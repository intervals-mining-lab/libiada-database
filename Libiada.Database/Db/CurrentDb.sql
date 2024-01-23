--23.01.2024 18:32:53

BEGIN;

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';

CREATE FUNCTION public.check_element_in_alphabet(chain_id bigint, element_id bigint) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
BEGIN
RETURN (SELECT count(*) = 1 
		FROM (SELECT alphabet a 
			  FROM chain 
			  WHERE id = chain_id) c 
		WHERE c.a @> ARRAY[element_id]);
END
$$;

COMMENT ON FUNCTION public.check_element_in_alphabet(chain_id bigint, element_id bigint) IS 'Checks if element with given id is present in alphabet of given sequence.';

CREATE FUNCTION public.copy_constraints(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  constrs record;
  srctable text;
  dsttable text;
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for constrs in
  select conname as name, pg_get_constraintdef(oid) as definition
  from pg_constraint where conrelid = srcoid loop
    begin
    execute 'alter table ' || dsttable
      || ' add constraint '
      || replace(replace(constrs.name, srctable, dsttable),'.','_')
      || ' ' || constrs.definition;
    i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION public.copy_constraints(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_constraints(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION public.copy_indexes(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  indexes record;
  srctable text;
  dsttable text;
  script text;
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for indexes in
  select c.relname as name, pg_get_indexdef(idx.indexrelid) as definition
  from pg_index idx, pg_class c where idx.indrelid = srcoid and c.oid = idx.indexrelid loop
    script = replace (indexes.definition, ' INDEX '
      || indexes.name, ' INDEX '
      || replace(replace(indexes.name, srctable, dsttable),'.','_'));
    script = replace (script, ' ON ' || srctable, ' ON ' || dsttable);
    begin
      execute script;
      i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION public.copy_indexes(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_indexes(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION public.copy_triggers(srcoid oid, dstoid oid) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare
  i int4 := 0;
  triggers record;
  srctable text;
  dsttable text;
  script text = '';
begin
  srctable = srcoid::regclass;
  dsttable = dstoid::regclass;
  for triggers in
   select tgname as name, pg_get_triggerdef(oid) as definition
   from pg_trigger where tgrelid = srcoid loop
    script =
    replace (triggers.definition, ' TRIGGER '
      || triggers.name, ' TRIGGER '
      || replace(replace(triggers.name, srctable, dsttable),'.','_'));
    script = replace (script, ' ON ' || srctable, ' ON ' || dsttable);
    begin
      execute script;
      i = i + 1;
    exception
      when duplicate_table then
    end;
  end loop;
  return i;
exception when undefined_table then
  return null;
end;
$$;

CREATE FUNCTION public.copy_triggers(src text, dst text) RETURNS integer
    LANGUAGE plpgsql IMMUTABLE
    AS $$
begin
  return copy_triggers(src::regclass::oid, dst::regclass::oid);
end;
$$;

CREATE FUNCTION public.db_integrity_test() RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
sequence_ids_count bigint;
distincs_sequence_ids_count bigint;
sequences_keys_disproportion integer;
elements_ids_count integer;
distinct_elements_count integer;
element_key_disproportion integer;
orphaned_elements_count integer;
orphaned_congeneric_characteristics integer;
orphaned_binary_characteristics integer;
orphaned_accordance_characteristics integer;
BEGIN
RAISE INFO 'Checking referential integrity of the database.';

RAISE INFO 'Checking "sequence" table and its children.';
SELECT COUNT(s.id) INTO sequence_ids_count FROM (SELECT id FROM chain UNION ALL SELECT id FROM subsequence) s;
SELECT COUNT(DISTINCT s.id) INTO distincs_sequence_ids_count FROM (SELECT id FROM chain UNION SELECT id FROM subsequence) s;
IF sequence_ids_count != distincs_sequence_ids_count THEN
	RAISE EXCEPTION  'Ids in "sequence" table and/or its cildren are not unique.';
ELSE
	RAISE INFO 'All sequence ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in "sequence" table and its children to the records in sequence_key table.';
SELECT COUNT(*) INTO sequences_keys_disproportion 
	FROM (SELECT id FROM chain UNION ALL SELECT id FROM subsequence) s 
	FULL OUTER JOIN chain_key sk ON sk.id = s.id 
	WHERE s.id IS NULL OR sk.id IS NULL;
IF sequences_keys_disproportion > 0 THEN
	RAISE EXCEPTION 'Number of records in sequence_key is not equal to number of records in sequence table and its children.';
ELSE
	RAISE INFO 'sequence_key is in sync with sequence table and its children.';
END IF;

RAISE INFO 'Sequences tables are all checked.';

RAISE INFO 'Checking "element" table and its children.';
SELECT COUNT(id) INTO elements_ids_count FROM element;
SELECT COUNT(DISTINCT id) INTO distinct_elements_count FROM element;
IF elements_ids_count != distinct_elements_count THEN
	RAISE EXCEPTION 'ids in "element" table and/or its cildren are not unique.';
ELSE
	RAISE INFO 'All element ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in "element" table and its children to the records in element_key table.';
SELECT COUNT (*) INTO element_key_disproportion 
	FROM (SELECT e.id, ek.id  FROM element e 
		  FULL OUTER JOIN element_key ek 
		  ON ek.id = e.id 
		  WHERE e.id IS NULL OR ek.id IS NULL) ec;
IF element_key_disproportion > 0 THEN
	RAISE EXCEPTION 'Number of records in element_key is not equal to number of records in element and its children.';
ELSE
	RAISE INFO 'element_key is in sync with element and its children.';
END IF;

RAISE INFO 'Elements tables are all checked.';

RAISE INFO 'Checking alphabets of all sequences.';
SELECT COUNT(c.a) INTO orphaned_elements_count 
	FROM (SELECT DISTINCT unnest(alphabet) a FROM chain) c 
		  LEFT OUTER JOIN element_key e 
		  ON e.id = c.a 
		  WHERE e.id IS NULL;
IF orphaned_elements_count > 0 THEN 
	RAISE EXCEPTION 'There are % missing elements of alphabet.', orphaned_elements_count;
ELSE
	RAISE INFO 'All alphabets elements are present in element_key table.';
END IF;

SELECT COUNT(cc.id) INTO orphaned_congeneric_characteristics
	FROM congeneric_characteristic cc 
	LEFT OUTER JOIN
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON c.id = cc.chain_id AND c.a = cc.element_id 
	WHERE c.a IS NULL;
IF orphaned_congeneric_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned congeneric characteristics without according elements in alphabets.', orphaned_congeneric_characteristics;
ELSE
	RAISE INFO 'All congeneric characteristics have corresponding elements in alphabets.';
END IF;

SELECT COUNT(bc.id) INTO orphaned_binary_characteristics
	FROM binary_characteristic bc 
	LEFT OUTER JOIN 
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON c.id = bc.chain_id AND (c.a = bc.first_element_id OR c.a = bc.second_element_id)
	WHERE c.a IS NULL;
IF orphaned_binary_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned binary characteristics without according elements in alphabets.', orphaned_binary_characteristics;
ELSE
	RAISE INFO 'All binary characteristics have corresponding elements in alphabets.';
END IF;

SELECT COUNT(ac.id) INTO orphaned_accordance_characteristics
	FROM accordance_characteristic ac 
	LEFT OUTER JOIN 
		(SELECT unnest(alphabet) a, id FROM chain) c 
	ON (c.id = ac.first_chain_id AND c.a = ac.first_element_id) OR(c.id = ac.second_chain_id AND c.a = ac.second_element_id)
	WHERE c.a IS NULL;
IF orphaned_accordance_characteristics > 0 THEN
	RAISE EXCEPTION 'There are % orphaned accordance characteristics without according elements in alphabets.', orphaned_accordance_characteristics;
ELSE
	RAISE INFO 'All accordance characteristics have corresponding elements in alphabets.';
END IF;

RAISE INFO 'All alphabets are checked.';

RAISE INFO 'Referential integrity of database is successfully checked.';
END
$$;

COMMENT ON FUNCTION public.db_integrity_test() IS 'Procedure for cheking referential integrity of the database.';

CREATE FUNCTION public.trigger_building_check() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
max_value integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	IF NEW.building[1] != 1 THEN
		RAISE EXCEPTION  'First order value is not 1. Actual value %.', NEW.building[1];
	END IF;
	max_value := 0;
	FOR i IN array_lower(NEW.building, 1)..array_upper(NEW.building, 1) LOOP
		IF NEW.building[i] > (max_value + 1) THEN
			RAISE EXCEPTION  'Order is incorrect starting from % position.', i ;
		END IF;
		IF NEW.building[i] = (max_value + 1) THEN
			max_value := NEW.building[i];
		END IF;
	END LOOP;
	IF max_value != array_length(NEW.alphabet, 1) THEN
		RAISE EXCEPTION  'Alphabet size is not equal to the order maximum value. Alphabet elements count %, and order max value %.', array_length(NEW.alphabet, 1), max_value;
	END IF;
	RETURN NEW;
ELSE
	RAISE EXCEPTION  'Unknown operation. This trigger only operates on INSERT operation on tables with building column.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_building_check() IS 'Validates inner consistency of the order of given sequence.';

CREATE FUNCTION public.trigger_chain_key_bound() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'INSERT' THEN
	INSERT INTO chain_key VALUES (NEW.id);
	return NEW;
ELSE IF TG_OP = 'UPDATE' AND NEW.id != OLD.id THEN
	UPDATE chain_key SET id = NEW.id WHERE id = OLD.id;
	return NEW;
ELSE IF TG_OP = 'DELETE' THEN
	DELETE FROM chain_key WHERE id = OLD.id;
	return OLD;
END IF;
END IF;
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_chain_key_bound() IS 'Links insert, update and delete operations on sequences tables with chain_key table.';

CREATE FUNCTION public.trigger_chain_key_unique_check() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
sequence_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO sequence_with_id_count FROM(
		SELECT id FROM chain WHERE id = NEW.id 
		UNION ALL 
		SELECT id FROM subsequence WHERE id = NEW.id
		UNION ALL 
		SELECT id FROM image_sequence WHERE id = NEW.id) s;
	IF sequence_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF sequence_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there is no sequences with given id.';
	END IF;
		RAISE EXCEPTION 'New record in table chain_key cannot be addded because there more than one sequences with given id. Sequences count = %', sequence_with_id_count;
	END IF;
ELSE	
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT operation on tables with id column.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_chain_key_unique_check() IS 'Checks that there is one and only one sequence with the given id.';

CREATE FUNCTION public.trigger_check_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
	orphaned_elements integer;
	alphabet_elemnts_not_unique bool;
BEGIN
	IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
		SELECT a.dc != a.ec INTO alphabet_elemnts_not_unique FROM (SELECT COUNT(DISTINCT e) dc, COUNT(e) ec FROM unnest(NEW.alphabet) e) a;
		IF alphabet_elemnts_not_unique THEN
			RAISE EXCEPTION 'Alphabet elements are not unique. Alphabet %', NEW.alphabet ;
		END IF;
		
		SELECT count(1) INTO orphaned_elements result FROM unnest(NEW.alphabet) a LEFT OUTER JOIN element_key e ON e.id = a WHERE e.id IS NULL;
		IF orphaned_elements != 0 THEN 
			RAISE EXCEPTION 'There are % elements of the alphabet missing in database.', orphaned_elements;
		END IF;
		
		RETURN NEW;
	END IF;
    RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with alphabet column (of array type).';
END;
$$;

COMMENT ON FUNCTION public.trigger_check_alphabet() IS 'Trigger function checking that all alphabet elements of the sequencs are in database.';

CREATE FUNCTION public.trigger_check_element_in_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.element_id);
	IF element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element (id = %) not present in sequence (id = %) alphabet.', NEW.element_id, NEW.chain_id;
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with chain_id, element_id.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_check_element_in_alphabet() IS 'Checks if element of congeneric characteristic is present in alphabet of corresponding sequence. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE FUNCTION public.trigger_check_elements_in_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
first_element_in_alphabet bool;
second_element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	first_element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.first_element_id);
	second_element_in_alphabet := check_element_in_alphabet(NEW.chain_id, NEW.second_element_id);
	IF first_element_in_alphabet AND second_element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element or elements (first_id = %, second_id = %) not present in sequence (id = %) alphabet.', NEW.first_element_id, NEW.second_element_id, NEW.chain_id;
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with chain_id, first_element_id, second_element_id.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_check_elements_in_alphabet() IS 'Checks if elements of binary characteristic are present in alphabet of corresponding sequence. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE FUNCTION public.trigger_check_elements_in_alphabets() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
first_element_in_alphabet bool;
second_element_in_alphabet bool;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	first_element_in_alphabet := check_element_in_alphabet(NEW.first_chain_id, NEW.first_element_id);
	second_element_in_alphabet := check_element_in_alphabet(NEW.second_chain_id, NEW.second_element_id);
	IF first_element_in_alphabet AND second_element_in_alphabet THEN
		RETURN NEW;
	ELSE 
		RAISE EXCEPTION 'New characteristic is referencing element not present in sequence alphabet.';
	END IF;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with first_chain_id, second_chain_id, first_element_id, second_element_id.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_check_elements_in_alphabets() IS 'Checks if elements of accordance characteristics are present in alphabets of corresponding sequences. Essentialy this function serves as foregin key referencing alphabet of sequence.';

CREATE FUNCTION public.trigger_check_notes_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
	orphaned_elements integer;
	alphabet_elemnts_not_unique bool;
BEGIN
	IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
		SELECT a.dc != a.ec INTO alphabet_elemnts_not_unique FROM (SELECT COUNT(DISTINCT e) dc, COUNT(e) ec FROM unnest(NEW.alphabet) e) a;
		IF alphabet_elemnts_not_unique THEN
			RAISE EXCEPTION 'Alphabet notes are not unique. Alphabet %', NEW.alphabet ;
		END IF;
		
		SELECT count(1) INTO orphaned_elements result FROM unnest(NEW.alphabet) a LEFT OUTER JOIN note e ON e.id = a WHERE e.id IS NULL;
		IF orphaned_elements != 0 THEN 
			RAISE EXCEPTION 'There are % notes of the alphabet missing in database.', orphaned_elements;
		END IF;
		
		RETURN NEW;
	END IF;
    RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with alphabet column (of array type).';
END;
$$;

COMMENT ON FUNCTION public.trigger_check_notes_alphabet() IS 'Trigger function checking that all alphabet notes of the music sequences are in database.';

CREATE FUNCTION public.trigger_delete_chain_characteristics() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'UPDATE' THEN
	DELETE FROM full_characteristic WHERE full_characteristic.chain_id = OLD.id;
	DELETE FROM binary_characteristic WHERE binary_characteristic.chain_id = OLD.id;
	DELETE FROM congeneric_characteristic WHERE congeneric_characteristic.chain_id = OLD.id;
	DELETE FROM accordance_characteristic WHERE accordance_characteristic.first_chain_id = OLD.id OR accordance_characteristic.second_chain_id = OLD.id;
	RETURN NEW;
ELSE
	RAISE EXCEPTION 'Unknown operation. This trigger only works on UPDATE operation.';
END IF;
END;
$$;

COMMENT ON FUNCTION public.trigger_delete_chain_characteristics() IS 'Trigger function deleting all characteristics of sequences that has been updated.';

CREATE FUNCTION public.trigger_element_delete_alphabet_bound() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
element_used bool;
BEGIN
IF TG_OP = 'DELETE' THEN
	SELECT count(*) > 0 INTO element_used 
	FROM (SELECT DISTINCT unnest(alphabet) a 
		  FROM (SELECT alphabet FROM chain 
				UNION SELECT alphabet FROM fmotif 
				UNION SELECT alphabet FROM measure) c
		  WHERE c.alphabet @> ARRAY[OLD.id]) s;
	IF element_used THEN
		RAISE EXCEPTION 'Cannot delete element, because it still is in some of the sequences alphabets.';
	ELSE
		return OLD;
	END IF;
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in delete operation on tables with id field.';
END
$$;

COMMENT ON FUNCTION public.trigger_element_delete_alphabet_bound() IS 'Checks if there is still sequences with element to be deleted, and if there are such sequences it raises exception.';

CREATE FUNCTION public.trigger_element_key_bound() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'INSERT' THEN
	INSERT INTO element_key VALUES (NEW.id);
	return NEW;
ELSE IF TG_OP = 'UPDATE' AND NEW.id != OLD.id THEN
	UPDATE element_key SET id = NEW.id WHERE id = OLD.id;
	return NEW;
ELSE IF TG_OP = 'DELETE' THEN
	DELETE FROM element_key WHERE id = OLD.id;
	return OLD;
END IF;
END IF;
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_element_key_bound() IS 'Links insert, update and delete actions on element tables with element_key table.';

CREATE FUNCTION public.trigger_element_key_insert() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
elements_with_id_count integer;
BEGIN
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
	SELECT count(*) INTO elements_with_id_count FROM element WHERE id = NEW.id;
	IF elements_with_id_count = 1 THEN
		RETURN NEW;
	ELSE IF elements_with_id_count = 0 THEN
		RAISE EXCEPTION 'New record in table element_key cannot be addded because there is no elements with given id = %.', NEW.id;
	END IF;
		RAISE EXCEPTION 'New record in table element_key cannot be addded because there more than one elements with given id = %.', NEW.id;
	END IF;
	RAISE EXCEPTION 'Cannot add record into element_key before adding record into element table or its child.';
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger only works on insert into table with id field.';
END
$$;

COMMENT ON FUNCTION public.trigger_element_key_insert() IS 'Adds new element id into element_key table.';

CREATE FUNCTION public.trigger_element_update_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'UPDATE' THEN
	UPDATE chain SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM chain c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE chain.id = c1.id;
	UPDATE fmotif SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM fmotif c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE fmotif.id = c1.id;
	UPDATE measure SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM measure c1 WHERE alphabet @> ARRAY[OLD.id]) c1 WHERE measure.id = c1.id;
	
	RETURN NEW;
END IF; 
RAISE EXCEPTION 'Unknown operation. This trigger is only meat for update operations on tables with alphabet field';
END
$$;

COMMENT ON FUNCTION public.trigger_element_update_alphabet() IS 'Automaticly updates elements ids in sequences alphabet when ids are changed in element table.';

CREATE FUNCTION public.trigger_set_modified() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
NEW.modified := now();
IF TG_OP = 'INSERT' THEN
	NEW.created := now();
	RETURN NEW;
END IF;
IF TG_OP = 'UPDATE' THEN
	NEW.created := OLD.created;
	RETURN NEW;
END IF;
	RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on tables with modified and created columns.';
END;
$$;

COMMENT ON FUNCTION public.trigger_set_modified() IS 'Rewrites created and modified columns with current values.';

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE public."AspNetPushNotificationSubscribers" (
    "Id" integer NOT NULL,
    "UserId" integer NOT NULL,
    "Endpoint" text NOT NULL,
    "P256dh" text NOT NULL,
    "Auth" text NOT NULL
);

COMMENT ON TABLE public."AspNetPushNotificationSubscribers" IS 'Table for storing data about devices that are subscribers to push notifications.';

ALTER TABLE public."AspNetPushNotificationSubscribers" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetPushNotificationSubscribers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" integer NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public."AspNetRoles" (
    "Id" integer NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);

ALTER TABLE public."AspNetRoles" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoles_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" integer NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" character varying(128) NOT NULL,
    "ProviderKey" character varying(128) NOT NULL,
    "ProviderDisplayName" text,
    "UserId" integer NOT NULL
);

CREATE TABLE public."AspNetUserRoles" (
    "UserId" integer NOT NULL,
    "RoleId" integer NOT NULL
);

CREATE TABLE public."AspNetUserTokens" (
    "UserId" integer NOT NULL,
    "LoginProvider" character varying(128) NOT NULL,
    "Name" character varying(128) NOT NULL,
    "Value" text
);

CREATE TABLE public."AspNetUsers" (
    "Id" integer NOT NULL,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);

ALTER TABLE public."AspNetUsers" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUsers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);

CREATE TABLE public.accordance_characteristic (
    id bigint NOT NULL,
    first_chain_id bigint NOT NULL,
    second_chain_id bigint NOT NULL,
    value double precision NOT NULL,
    first_element_id bigint NOT NULL,
    second_element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.accordance_characteristic IS 'Contains numeric chracteristics of accordance of element in different sequences.';

COMMENT ON COLUMN public.accordance_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.accordance_characteristic.first_chain_id IS 'Id of the first sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.second_chain_id IS 'Id of the second sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.accordance_characteristic.first_element_id IS 'Id of the element of the first sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.second_element_id IS 'Id of the element of the second sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.characteristic_link_id IS 'Characteristic type id.';

CREATE SEQUENCE public.accordance_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.accordance_characteristic_id_seq OWNED BY public.accordance_characteristic.id;

CREATE TABLE public.accordance_characteristic_link (
    id smallint NOT NULL,
    accordance_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    CONSTRAINT accordance_characteristic_check CHECK (((accordance_characteristic)::integer <@ int4range(1, 2, '[]'::text))),
    CONSTRAINT accordance_characteristic_link_check CHECK (((link)::integer <@ int4range(0, 7, '[]'::text)))
);

COMMENT ON TABLE public.accordance_characteristic_link IS 'Contatins list of possible combinations of accordance characteristics parameters.';

COMMENT ON COLUMN public.accordance_characteristic_link.id IS 'Unique identifier.';

COMMENT ON COLUMN public.accordance_characteristic_link.accordance_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.accordance_characteristic_link.link IS 'Link enum numeric value.';

CREATE SEQUENCE public.accordance_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.accordance_characteristic_link_id_seq OWNED BY public.accordance_characteristic_link.id;

CREATE TABLE public.binary_characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    value double precision NOT NULL,
    first_element_id bigint NOT NULL,
    second_element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.binary_characteristic IS 'Contains numeric chracteristics of elements dependece based on their arrangement in sequence.';

COMMENT ON COLUMN public.binary_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.binary_characteristic.chain_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.binary_characteristic.first_element_id IS 'Id of the first element of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.second_element_id IS 'Id of the second element of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.characteristic_link_id IS 'Characteristic type id.';

CREATE SEQUENCE public.binary_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.binary_characteristic_id_seq OWNED BY public.binary_characteristic.id;

CREATE TABLE public.binary_characteristic_link (
    id smallint NOT NULL,
    binary_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    CONSTRAINT binary_characteristic_check CHECK (((binary_characteristic)::integer <@ int4range(1, 6, '[]'::text))),
    CONSTRAINT binary_characteristic_link_check CHECK (((link)::integer <@ int4range(0, 7, '[]'::text)))
);

COMMENT ON TABLE public.binary_characteristic_link IS 'Contatins list of possible combinations of dependence characteristics parameters.';

COMMENT ON COLUMN public.binary_characteristic_link.id IS 'Unique identifier.';

COMMENT ON COLUMN public.binary_characteristic_link.binary_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.binary_characteristic_link.link IS 'Link enum numeric value.';

CREATE SEQUENCE public.binary_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.binary_characteristic_link_id_seq OWNED BY public.binary_characteristic_link.id;

CREATE TABLE public.element (
    id bigint NOT NULL,
    value character varying(255),
    description text,
    name character varying(255),
    notation smallint NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON TABLE public.element IS 'Base table for all elements that are stored in the database and used in alphabets of sequences.';

COMMENT ON COLUMN public.element.id IS 'Unique internal identifier of the element.';

COMMENT ON COLUMN public.element.value IS 'Content of the element.';

COMMENT ON COLUMN public.element.description IS 'Description of the element.';

COMMENT ON COLUMN public.element.name IS 'Name of the element.';

COMMENT ON COLUMN public.element.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.element.created IS 'Element creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.element.modified IS 'Record last change date and time (updated trough trigger).';

CREATE SEQUENCE public.elements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.elements_id_seq OWNED BY public.element.id;

CREATE TABLE public.chain (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass) NOT NULL,
    notation smallint NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    matter_id bigint NOT NULL,
    alphabet bigint[] NOT NULL,
    building integer[] NOT NULL,
    remote_id character varying(255),
    remote_db smallint,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    description text,
    CONSTRAINT chk_remote_id CHECK ((((remote_db IS NULL) AND (remote_id IS NULL)) OR ((remote_db IS NOT NULL) AND (remote_id IS NOT NULL))))
);

COMMENT ON TABLE public.chain IS 'Base table for all sequences that are stored in the database as alphabet and order.';

COMMENT ON COLUMN public.chain.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.chain.notation IS 'Notation of the sequence (words, letters, notes, nucleotides, etc.).';

COMMENT ON COLUMN public.chain.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.chain.matter_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.chain.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.chain.building IS 'Sequence''s order.';

COMMENT ON COLUMN public.chain.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.chain.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.chain.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.chain.description IS 'Description of the sequence.';

CREATE TABLE public.chain_attribute (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    attribute smallint NOT NULL,
    value text NOT NULL
);

COMMENT ON TABLE public.chain_attribute IS 'Contains chains'' attributes and their values.';

COMMENT ON COLUMN public.chain_attribute.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.chain_attribute.chain_id IS 'Id of the sequence to which attribute belongs.';

COMMENT ON COLUMN public.chain_attribute.attribute IS 'Attribute enum numeric value.';

COMMENT ON COLUMN public.chain_attribute.value IS 'Text of the attribute.';

CREATE SEQUENCE public.chain_attribute_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.chain_attribute_id_seq OWNED BY public.chain_attribute.id;

CREATE TABLE public.chain_key (
    id bigint NOT NULL
);

COMMENT ON TABLE public.chain_key IS 'Surrogate table that contains keys for all sequences tables and used for foreign key references.';

COMMENT ON COLUMN public.chain_key.id IS 'Unique identifier of the sequence used in other tables. Surrogate for foreign keys.';

CREATE TABLE public.full_characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    value double precision NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.full_characteristic IS 'Contains numeric chracteristics of complete sequences.';

COMMENT ON COLUMN public.full_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.full_characteristic.chain_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.full_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.full_characteristic.characteristic_link_id IS 'Characteristic type id.';

CREATE SEQUENCE public.characteristics_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.characteristics_id_seq OWNED BY public.full_characteristic.id;

CREATE TABLE public.congeneric_characteristic (
    id bigint NOT NULL,
    chain_id bigint NOT NULL,
    value double precision NOT NULL,
    element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.congeneric_characteristic IS 'Contains numeric chracteristics of congeneric sequences.';

COMMENT ON COLUMN public.congeneric_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.congeneric_characteristic.chain_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.congeneric_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.congeneric_characteristic.element_id IS 'Id of the element for which the characteristic is calculated.';

COMMENT ON COLUMN public.congeneric_characteristic.characteristic_link_id IS 'Characteristic type id.';

CREATE SEQUENCE public.congeneric_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.congeneric_characteristic_id_seq OWNED BY public.congeneric_characteristic.id;

CREATE TABLE public.congeneric_characteristic_link (
    id smallint NOT NULL,
    congeneric_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    arrangement_type smallint DEFAULT 0 NOT NULL,
    CONSTRAINT congeneric_characteristic_link_check CHECK (((link)::integer <@ int4range(0, 7, '[]'::text)))
);

COMMENT ON TABLE public.congeneric_characteristic_link IS 'Contatins list of possible combinations of congeneric characteristics parameters.';

COMMENT ON COLUMN public.congeneric_characteristic_link.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.congeneric_characteristic_link.congeneric_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.congeneric_characteristic_link.link IS 'Link enum numeric value.';

COMMENT ON COLUMN public.congeneric_characteristic_link.arrangement_type IS 'Arrangement type enum numeric value.';

CREATE SEQUENCE public.congeneric_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.congeneric_characteristic_link_id_seq OWNED BY public.congeneric_characteristic_link.id;

CREATE TABLE public.data_chain (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass),
    notation smallint,
    created timestamp with time zone DEFAULT now(),
    matter_id bigint,
    alphabet bigint[],
    building integer[],
    remote_id character varying(255),
    remote_db smallint,
    modified timestamp with time zone DEFAULT now(),
    description text,
    CONSTRAINT chk_remote_id CHECK ((((remote_db IS NULL) AND (remote_id IS NULL)) OR ((remote_db IS NOT NULL) AND (remote_id IS NOT NULL))))
)
INHERITS (public.chain);

COMMENT ON TABLE public.data_chain IS 'Contains sequences that represent time series and other ordered data arrays.';

COMMENT ON COLUMN public.data_chain.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.data_chain.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.data_chain.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.data_chain.matter_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.data_chain.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.data_chain.building IS 'Sequence''s order.';

COMMENT ON COLUMN public.data_chain.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.data_chain.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.data_chain.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.data_chain.description IS 'Description of the sequence.';

CREATE TABLE public.dna_chain (
    partial boolean DEFAULT false NOT NULL
)
INHERITS (public.chain);

COMMENT ON TABLE public.dna_chain IS 'Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc).';

COMMENT ON COLUMN public.dna_chain.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.dna_chain.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.dna_chain.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.dna_chain.matter_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.dna_chain.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.dna_chain.building IS 'Sequence''s order.';

COMMENT ON COLUMN public.dna_chain.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.dna_chain.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.dna_chain.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.dna_chain.description IS 'Description of the sequence.';

COMMENT ON COLUMN public.dna_chain.partial IS 'Flag indicating whether sequence is partial or complete.';

CREATE TABLE public.element_key (
    id bigint NOT NULL
);

COMMENT ON TABLE public.element_key IS 'Surrogate table that contains keys for all elements tables and used for foreign key references.';

COMMENT ON COLUMN public.element_key.id IS 'Unique identifier of the element used in other tables. Surrogate for foreign keys.';

CREATE TABLE public.fmotif (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass),
    value character varying(255),
    description text,
    name character varying(255),
    notation smallint DEFAULT 6,
    created timestamp with time zone DEFAULT now(),
    modified timestamp with time zone DEFAULT now(),
    alphabet bigint[] NOT NULL,
    building integer[] NOT NULL,
    fmotif_type smallint NOT NULL
)
INHERITS (public.element);

COMMENT ON TABLE public.fmotif IS 'Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.';

COMMENT ON COLUMN public.fmotif.id IS 'Unique internal identifier of the fmotif.';

COMMENT ON COLUMN public.fmotif.value IS 'Fmotif hash value.';

COMMENT ON COLUMN public.fmotif.description IS 'Fmotif description.';

COMMENT ON COLUMN public.fmotif.name IS 'Fmotif name.';

COMMENT ON COLUMN public.fmotif.notation IS 'Fmotif notation enum numeric value (always 6).';

COMMENT ON COLUMN public.fmotif.created IS 'Fmotif creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.fmotif.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.fmotif.alphabet IS 'Fmotif''s alphabet (array of notes ids).';

COMMENT ON COLUMN public.fmotif.building IS 'Fmotif''s order.';

COMMENT ON COLUMN public.fmotif.fmotif_type IS 'Fmotif type enum numeric value.';

CREATE TABLE public.full_characteristic_link (
    id smallint NOT NULL,
    full_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    arrangement_type smallint DEFAULT 0 NOT NULL,
    CONSTRAINT full_characteristic_link_check CHECK (((link)::integer <@ int4range(0, 7, '[]'::text)))
);

COMMENT ON TABLE public.full_characteristic_link IS 'Contatins list of possible combinations of characteristics parameters.';

COMMENT ON COLUMN public.full_characteristic_link.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.full_characteristic_link.full_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.full_characteristic_link.link IS 'Link enum numeric value.';

COMMENT ON COLUMN public.full_characteristic_link.arrangement_type IS 'Arrangement type enum numeric value.';

CREATE SEQUENCE public.full_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.full_characteristic_link_id_seq OWNED BY public.full_characteristic_link.id;

CREATE TABLE public.image_sequence (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass) NOT NULL,
    notation smallint NOT NULL,
    order_extractor smallint NOT NULL,
    image_transformations smallint[] NOT NULL,
    matrix_transformations smallint[] NOT NULL,
    matter_id bigint NOT NULL,
    remote_id text,
    remote_db smallint,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT chk_remote_id CHECK ((((remote_db IS NULL) AND (remote_id IS NULL)) OR ((remote_db IS NOT NULL) AND (remote_id IS NOT NULL))))
);

COMMENT ON TABLE public.image_sequence IS 'Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.';

COMMENT ON COLUMN public.image_sequence.id IS 'Unique internal identifier of the image sequence.';

COMMENT ON COLUMN public.image_sequence.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.image_sequence.order_extractor IS 'Order extractor enum numeric value used in the process of creation of the sequence.';

COMMENT ON COLUMN public.image_sequence.image_transformations IS 'Array of image transformations applied begore the extraction of the sequence.';

COMMENT ON COLUMN public.image_sequence.matrix_transformations IS 'Array of matrix transformations applied begore the extraction of the sequence.';

COMMENT ON COLUMN public.image_sequence.matter_id IS 'Id of the research object (image) to which the sequence belongs.';

COMMENT ON COLUMN public.image_sequence.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.image_sequence.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.image_sequence.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.image_sequence.modified IS 'Record last change date and time (updated trough trigger).';

CREATE TABLE public.literature_chain (
    original boolean DEFAULT true NOT NULL,
    language smallint NOT NULL,
    translator smallint DEFAULT 0 NOT NULL,
    CONSTRAINT chk_original_translator CHECK (((original AND (translator = 0)) OR (NOT original)))
)
INHERITS (public.chain);

COMMENT ON TABLE public.literature_chain IS 'Contains sequences that represent literary works and their various translations.';

COMMENT ON COLUMN public.literature_chain.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.literature_chain.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.literature_chain.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.literature_chain.matter_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.literature_chain.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.literature_chain.building IS 'Sequence''s order.';

COMMENT ON COLUMN public.literature_chain.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.literature_chain.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.literature_chain.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.literature_chain.description IS 'Sequence description.';

COMMENT ON COLUMN public.literature_chain.original IS 'Flag indicating if this sequence is in original language or was translated.';

COMMENT ON COLUMN public.literature_chain.language IS 'Primary language of literary work.';

COMMENT ON COLUMN public.literature_chain.translator IS 'Author of translation or automated translator.';

CREATE TABLE public.matter (
    id bigint NOT NULL,
    name text NOT NULL,
    nature smallint NOT NULL,
    description text,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    sequence_type smallint NOT NULL,
    "group" smallint NOT NULL,
    multisequence_id integer,
    multisequence_number smallint,
    source bytea,
    collection_country text,
    collection_date date,
    collection_location text,
    CONSTRAINT chk_multisequence_reference CHECK ((((multisequence_id IS NULL) AND (multisequence_number IS NULL)) OR ((multisequence_id IS NOT NULL) AND (multisequence_number IS NOT NULL))))
);

COMMENT ON TABLE public.matter IS 'Contains research objects, samples, texts, etc (one research object may be represented by several sequences).';

COMMENT ON COLUMN public.matter.id IS 'Unique internal identifier of the research object.';

COMMENT ON COLUMN public.matter.name IS 'Research object name.';

COMMENT ON COLUMN public.matter.nature IS 'Research object nature enum numeric value.';

COMMENT ON COLUMN public.matter.description IS 'Description of the research object.';

COMMENT ON COLUMN public.matter.created IS 'Record creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.matter.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.matter.sequence_type IS 'Sequence type enum numeric value.';

COMMENT ON COLUMN public.matter."group" IS 'Group enum numeric value.';

COMMENT ON COLUMN public.matter.multisequence_id IS 'Id of the parent multisequence.';

COMMENT ON COLUMN public.matter.multisequence_number IS 'Serial number in multisequence.';

COMMENT ON COLUMN public.matter.source IS 'Source of the genetic sequence.';

COMMENT ON COLUMN public.matter.collection_country IS 'Collection country of the genetic sequence.';

COMMENT ON COLUMN public.matter.collection_date IS 'Collection date of the genetic sequence.';

COMMENT ON COLUMN public.matter.collection_location IS 'Collection location of the genetic sequence.';

CREATE SEQUENCE public.matter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.matter_id_seq OWNED BY public.matter.id;

CREATE TABLE public.measure (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass),
    value character varying(255),
    description text,
    name character varying(255),
    notation smallint DEFAULT 7,
    alphabet bigint[] NOT NULL,
    building integer[] NOT NULL,
    beats integer NOT NULL,
    beatbase integer NOT NULL,
    fifths integer NOT NULL,
    major boolean NOT NULL
)
INHERITS (public.element);

COMMENT ON TABLE public.measure IS 'Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.';

COMMENT ON COLUMN public.measure.id IS 'Unique internal identifier of the measure.';

COMMENT ON COLUMN public.measure.value IS 'Measure hash code.';

COMMENT ON COLUMN public.measure.description IS 'Description of the sequence.';

COMMENT ON COLUMN public.measure.name IS 'Measure name.';

COMMENT ON COLUMN public.measure.notation IS 'Measure notation enum numeric value (always 7).';

COMMENT ON COLUMN public.measure.created IS 'Measure creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.measure.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.measure.alphabet IS 'Measure alphabet (array of notes ids).';

COMMENT ON COLUMN public.measure.building IS 'Measure order.';

COMMENT ON COLUMN public.measure.beats IS 'Time signature upper numeral (Beat numerator).';

COMMENT ON COLUMN public.measure.beatbase IS 'Time signature lower numeral (Beat denominator).';

COMMENT ON COLUMN public.measure.fifths IS 'Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).';

COMMENT ON COLUMN public.measure.major IS 'Music mode of the measure. true  represents major and false represents minor.';

CREATE TABLE public.multisequence (
    id integer NOT NULL,
    name text NOT NULL,
    nature smallint DEFAULT 1 NOT NULL
);

COMMENT ON TABLE public.multisequence IS 'Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups.';

COMMENT ON COLUMN public.multisequence.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.multisequence.name IS 'Multisequence name.';

COMMENT ON COLUMN public.multisequence.nature IS 'Multisequence nature enum numeric value.';

CREATE SEQUENCE public.multisequence_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.multisequence_id_seq OWNED BY public.multisequence.id;

CREATE TABLE public.music_chain (
    pause_treatment smallint DEFAULT 0 NOT NULL,
    sequential_transfer boolean DEFAULT false NOT NULL,
    CONSTRAINT chk_pause_treatment_and_sequential_transfer CHECK ((((notation = 6) AND (pause_treatment <> 0)) OR (((notation = 7) OR (notation = 8)) AND (pause_treatment = 0) AND (NOT sequential_transfer))))
)
INHERITS (public.chain);

COMMENT ON TABLE public.music_chain IS 'Contains sequences that represent musical works in form of note, fmotive or measure sequences.';

COMMENT ON COLUMN public.music_chain.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.music_chain.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.music_chain.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.music_chain.matter_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.music_chain.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.music_chain.building IS 'Sequence''s order.';

COMMENT ON COLUMN public.music_chain.remote_id IS 'Id of the sequence in the remote database.';

COMMENT ON COLUMN public.music_chain.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.music_chain.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.music_chain.description IS 'Sequence description.';

COMMENT ON COLUMN public.music_chain.pause_treatment IS 'Pause treatment enum numeric value.';

COMMENT ON COLUMN public.music_chain.sequential_transfer IS 'Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.';

CREATE TABLE public.note (
    notation smallint DEFAULT 8,
    numerator integer NOT NULL,
    denominator integer NOT NULL,
    triplet boolean NOT NULL,
    tie smallint DEFAULT 0 NOT NULL
)
INHERITS (public.element);

COMMENT ON TABLE public.note IS 'Contains elements that represent notes that are used as elements of music sequences.';

COMMENT ON COLUMN public.note.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.note.value IS 'Note hash code.';

COMMENT ON COLUMN public.note.description IS 'Note description.';

COMMENT ON COLUMN public.note.name IS 'Note name.';

COMMENT ON COLUMN public.note.notation IS 'Measure notation enum numeric value (always 8).';

COMMENT ON COLUMN public.note.created IS 'Measure creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.note.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.note.numerator IS 'Note duration fraction numerator.';

COMMENT ON COLUMN public.note.denominator IS 'Note duration fraction denominator.';

COMMENT ON COLUMN public.note.triplet IS 'Flag indicating if note is a part of triplet (tuplet).';

COMMENT ON COLUMN public.note.tie IS 'Note tie type enum numeric value.';

CREATE TABLE public.note_pitch (
    note_id bigint NOT NULL,
    pitch_id integer NOT NULL
);

COMMENT ON TABLE public.note_pitch IS 'Intermediate table representing M:M relationship between note and pitch.';

COMMENT ON COLUMN public.note_pitch.note_id IS 'Note id.';

COMMENT ON COLUMN public.note_pitch.pitch_id IS 'Pitch id.';

CREATE TABLE public."position" (
    id bigint NOT NULL,
    subsequence_id bigint NOT NULL,
    start integer NOT NULL,
    length integer NOT NULL
);

COMMENT ON TABLE public."position" IS 'Contains information on additional fragment positions (for subsequences concatenated from several parts).';

COMMENT ON COLUMN public."position".id IS 'Unique internal identifier.';

COMMENT ON COLUMN public."position".subsequence_id IS 'Parent subsequence id.';

COMMENT ON COLUMN public."position".start IS 'Index of the fragment beginning (from zero).';

COMMENT ON COLUMN public."position".length IS 'Fragment length.';

CREATE SEQUENCE public.piece_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.piece_id_seq OWNED BY public."position".id;

CREATE TABLE public.pitch (
    id integer NOT NULL,
    octave integer NOT NULL,
    midinumber integer NOT NULL,
    instrument smallint DEFAULT 0 NOT NULL,
    accidental smallint DEFAULT 0 NOT NULL,
    note_symbol smallint NOT NULL
);

COMMENT ON TABLE public.pitch IS 'Note''s pitch.';

COMMENT ON COLUMN public.pitch.id IS 'Unique internal identifier of the pitch.';

COMMENT ON COLUMN public.pitch.octave IS 'Octave number.';

COMMENT ON COLUMN public.pitch.midinumber IS 'Unique number by midi standard.';

COMMENT ON COLUMN public.pitch.instrument IS 'Pitch instrument enum numeric value.';

COMMENT ON COLUMN public.pitch.accidental IS 'Pitch key signature enum numeric value.';

COMMENT ON COLUMN public.pitch.note_symbol IS 'Note symbol enum numeric value.';

CREATE SEQUENCE public.pitch_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.pitch_id_seq OWNED BY public.pitch.id;

CREATE TABLE public.sequence_group (
    id integer NOT NULL,
    name text NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    creator_id integer NOT NULL,
    modified timestamp with time zone NOT NULL,
    modifier_id integer NOT NULL,
    nature smallint NOT NULL,
    sequence_group_type smallint
);

COMMENT ON TABLE public.sequence_group IS 'Contains information about sequences groups.';

COMMENT ON COLUMN public.sequence_group.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.sequence_group.name IS 'Sequences group name.';

COMMENT ON COLUMN public.sequence_group.created IS 'Sequence group creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.sequence_group.creator_id IS 'Record creator user id.';

COMMENT ON COLUMN public.sequence_group.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.sequence_group.modifier_id IS 'Record editor user id.';

COMMENT ON COLUMN public.sequence_group.nature IS 'Sequences group nature enum numeric value.';

COMMENT ON COLUMN public.sequence_group.sequence_group_type IS 'Sequence group type enum numeric value.';

CREATE SEQUENCE public.sequence_group_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.sequence_group_id_seq OWNED BY public.sequence_group.id;

CREATE TABLE public.sequence_group_matter (
    group_id integer NOT NULL,
    matter_id bigint NOT NULL
);

COMMENT ON TABLE public.sequence_group_matter IS 'Intermediate table for infromation on matters belonging to groups.';

COMMENT ON COLUMN public.sequence_group_matter.group_id IS 'Sequence group id.';

COMMENT ON COLUMN public.sequence_group_matter.matter_id IS 'Research object id.';

CREATE TABLE public.subsequence (
    id bigint DEFAULT nextval('public.elements_id_seq'::regclass) NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    chain_id bigint NOT NULL,
    start integer NOT NULL,
    length integer NOT NULL,
    feature smallint NOT NULL,
    partial boolean DEFAULT false NOT NULL,
    remote_id character varying(255)
);

COMMENT ON TABLE public.subsequence IS 'Contains information on location and length of the fragments within complete sequences.';

COMMENT ON COLUMN public.subsequence.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.subsequence.created IS 'Sequence group creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.subsequence.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.subsequence.chain_id IS 'Parent sequence id.';

COMMENT ON COLUMN public.subsequence.start IS 'Index of the fragment beginning (from zero).';

COMMENT ON COLUMN public.subsequence.length IS 'Fragment length.';

COMMENT ON COLUMN public.subsequence.feature IS 'Subsequence feature enum numeric value.';

COMMENT ON COLUMN public.subsequence.partial IS 'Flag indicating whether subsequence is partial or complete.';

COMMENT ON COLUMN public.subsequence.remote_id IS 'Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).';

CREATE TABLE public.task (
    id bigint NOT NULL,
    task_type smallint NOT NULL,
    description text NOT NULL,
    status smallint NOT NULL,
    user_id integer NOT NULL,
    created timestamp with time zone NOT NULL,
    started timestamp with time zone,
    completed timestamp with time zone
);

COMMENT ON TABLE public.task IS 'Contains information about computational tasks.';

COMMENT ON COLUMN public.task.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.task.task_type IS 'Task type enum numeric value.';

COMMENT ON COLUMN public.task.description IS 'Task description.';

COMMENT ON COLUMN public.task.status IS 'Task status enum numeric value.';

COMMENT ON COLUMN public.task.user_id IS 'Creator user id.';

COMMENT ON COLUMN public.task.created IS 'Task creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.task.started IS 'Task beginning of computation date and time.';

COMMENT ON COLUMN public.task.completed IS 'Task completion date and time.';

CREATE SEQUENCE public.task_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.task_id_seq OWNED BY public.task.id;

CREATE TABLE public.task_result (
    id bigint NOT NULL,
    task_id bigint NOT NULL,
    key text NOT NULL,
    value json NOT NULL
);

COMMENT ON TABLE public.task_result IS 'Contains JSON results of tasks calculation. Results are stored as key/value pairs.';

COMMENT ON COLUMN public.task_result.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.task_result.task_id IS 'Parent task id.';

COMMENT ON COLUMN public.task_result.key IS 'Results element name.';

COMMENT ON COLUMN public.task_result.value IS 'Results element value (as json).';

CREATE SEQUENCE public.task_result_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.task_result_id_seq OWNED BY public.task_result.id;

ALTER TABLE ONLY public.accordance_characteristic ALTER COLUMN id SET DEFAULT nextval('public.accordance_characteristic_id_seq'::regclass);

ALTER TABLE ONLY public.accordance_characteristic_link ALTER COLUMN id SET DEFAULT nextval('public.accordance_characteristic_link_id_seq'::regclass);

ALTER TABLE ONLY public.binary_characteristic ALTER COLUMN id SET DEFAULT nextval('public.binary_characteristic_id_seq'::regclass);

ALTER TABLE ONLY public.binary_characteristic_link ALTER COLUMN id SET DEFAULT nextval('public.binary_characteristic_link_id_seq'::regclass);

ALTER TABLE ONLY public.chain_attribute ALTER COLUMN id SET DEFAULT nextval('public.chain_attribute_id_seq'::regclass);

ALTER TABLE ONLY public.congeneric_characteristic ALTER COLUMN id SET DEFAULT nextval('public.congeneric_characteristic_id_seq'::regclass);

ALTER TABLE ONLY public.congeneric_characteristic_link ALTER COLUMN id SET DEFAULT nextval('public.congeneric_characteristic_link_id_seq'::regclass);

ALTER TABLE ONLY public.dna_chain ALTER COLUMN id SET DEFAULT nextval('public.elements_id_seq'::regclass);

ALTER TABLE ONLY public.dna_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY public.dna_chain ALTER COLUMN remote_db SET DEFAULT 1;

ALTER TABLE ONLY public.dna_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY public.element ALTER COLUMN id SET DEFAULT nextval('public.elements_id_seq'::regclass);

ALTER TABLE ONLY public.full_characteristic ALTER COLUMN id SET DEFAULT nextval('public.characteristics_id_seq'::regclass);

ALTER TABLE ONLY public.full_characteristic_link ALTER COLUMN id SET DEFAULT nextval('public.full_characteristic_link_id_seq'::regclass);

ALTER TABLE ONLY public.literature_chain ALTER COLUMN id SET DEFAULT nextval('public.elements_id_seq'::regclass);

ALTER TABLE ONLY public.literature_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY public.literature_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY public.matter ALTER COLUMN id SET DEFAULT nextval('public.matter_id_seq'::regclass);

ALTER TABLE ONLY public.measure ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY public.measure ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY public.multisequence ALTER COLUMN id SET DEFAULT nextval('public.multisequence_id_seq'::regclass);

ALTER TABLE ONLY public.music_chain ALTER COLUMN id SET DEFAULT nextval('public.elements_id_seq'::regclass);

ALTER TABLE ONLY public.music_chain ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY public.music_chain ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY public.note ALTER COLUMN id SET DEFAULT nextval('public.elements_id_seq'::regclass);

ALTER TABLE ONLY public.note ALTER COLUMN created SET DEFAULT now();

ALTER TABLE ONLY public.note ALTER COLUMN modified SET DEFAULT now();

ALTER TABLE ONLY public.pitch ALTER COLUMN id SET DEFAULT nextval('public.pitch_id_seq'::regclass);

ALTER TABLE ONLY public."position" ALTER COLUMN id SET DEFAULT nextval('public.piece_id_seq'::regclass);

ALTER TABLE ONLY public.sequence_group ALTER COLUMN id SET DEFAULT nextval('public.sequence_group_id_seq'::regclass);

ALTER TABLE ONLY public.task ALTER COLUMN id SET DEFAULT nextval('public.task_id_seq'::regclass);

ALTER TABLE ONLY public.task_result ALTER COLUMN id SET DEFAULT nextval('public.task_result_id_seq'::regclass);

INSERT INTO public."AspNetRoles" VALUES (1, 'Admin', 'ADMIN', NULL);

INSERT INTO public.accordance_characteristic_link VALUES (1, 1, 2);
INSERT INTO public.accordance_characteristic_link VALUES (2, 1, 3);
INSERT INTO public.accordance_characteristic_link VALUES (3, 1, 6);
INSERT INTO public.accordance_characteristic_link VALUES (4, 1, 7);
INSERT INTO public.accordance_characteristic_link VALUES (5, 2, 2);
INSERT INTO public.accordance_characteristic_link VALUES (6, 2, 3);
INSERT INTO public.accordance_characteristic_link VALUES (7, 2, 6);
INSERT INTO public.accordance_characteristic_link VALUES (8, 2, 7);

INSERT INTO public.binary_characteristic_link VALUES (1, 1, 2);
INSERT INTO public.binary_characteristic_link VALUES (2, 1, 3);
INSERT INTO public.binary_characteristic_link VALUES (3, 1, 4);
INSERT INTO public.binary_characteristic_link VALUES (4, 2, 2);
INSERT INTO public.binary_characteristic_link VALUES (5, 2, 3);
INSERT INTO public.binary_characteristic_link VALUES (6, 2, 4);
INSERT INTO public.binary_characteristic_link VALUES (7, 3, 2);
INSERT INTO public.binary_characteristic_link VALUES (8, 3, 3);
INSERT INTO public.binary_characteristic_link VALUES (9, 3, 4);
INSERT INTO public.binary_characteristic_link VALUES (10, 4, 2);
INSERT INTO public.binary_characteristic_link VALUES (11, 4, 3);
INSERT INTO public.binary_characteristic_link VALUES (12, 4, 4);
INSERT INTO public.binary_characteristic_link VALUES (13, 5, 2);
INSERT INTO public.binary_characteristic_link VALUES (14, 5, 3);
INSERT INTO public.binary_characteristic_link VALUES (15, 5, 4);
INSERT INTO public.binary_characteristic_link VALUES (16, 6, 2);
INSERT INTO public.binary_characteristic_link VALUES (17, 6, 3);
INSERT INTO public.binary_characteristic_link VALUES (18, 6, 4);

INSERT INTO public.congeneric_characteristic_link VALUES (1, 1, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (2, 1, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (3, 1, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (4, 1, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (5, 1, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (6, 2, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (7, 2, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (8, 2, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (9, 2, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (10, 2, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (11, 3, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (12, 4, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (13, 5, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (14, 5, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (15, 5, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (16, 5, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (17, 5, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (18, 6, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (19, 6, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (20, 6, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (21, 6, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (22, 6, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (23, 7, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (24, 8, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (25, 8, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (26, 8, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (27, 8, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (28, 8, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (29, 9, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (30, 9, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (31, 9, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (32, 9, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (33, 9, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (34, 10, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (35, 10, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (36, 10, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (37, 10, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (38, 10, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (39, 11, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (40, 11, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (41, 11, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (42, 11, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (43, 11, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (44, 12, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (45, 13, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (46, 13, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (47, 13, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (48, 13, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (49, 13, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (50, 14, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (51, 15, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (52, 15, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (53, 15, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (54, 15, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (55, 15, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (56, 16, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (57, 16, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (58, 16, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (59, 16, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (60, 16, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (61, 17, 0, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (62, 18, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (63, 18, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (64, 18, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (65, 18, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (66, 18, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (67, 19, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (68, 19, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (69, 19, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (70, 19, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (71, 19, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (72, 20, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (73, 20, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (74, 20, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (75, 20, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (76, 20, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (77, 21, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (78, 21, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (79, 21, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (80, 21, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (81, 21, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (82, 22, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (83, 22, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (84, 22, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (85, 22, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (86, 22, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (87, 23, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (88, 23, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (89, 23, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (90, 23, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (91, 23, 5, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (92, 24, 1, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (93, 24, 2, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (94, 24, 3, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (95, 24, 4, 0);
INSERT INTO public.congeneric_characteristic_link VALUES (96, 24, 5, 0);

INSERT INTO public.element VALUES (54522389, 'A', NULL, 'Adenin', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522390, 'G', NULL, 'Guanine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522391, 'C', NULL, 'Cytosine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522392, 'T', NULL, 'Thymine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522393, 'U', NULL, 'Uracil ', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522394, 'TTT', NULL, 'TTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522395, 'GTA', NULL, 'GTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522396, 'CTG', NULL, 'CTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522397, 'TTA', NULL, 'TTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522398, 'GCG', NULL, 'GCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522399, 'ACC', NULL, 'ACC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522400, 'TCG', NULL, 'TCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522401, 'GAG', NULL, 'GAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522402, 'TAT', NULL, 'TAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522403, 'ACG', NULL, 'ACG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522404, 'AGG', NULL, 'AGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522405, 'ATA', NULL, 'ATA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522406, 'AAG', NULL, 'AAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522407, 'GAT', NULL, 'GAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522408, 'GAC', NULL, 'GAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522409, 'AAC', NULL, 'AAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522410, 'GAA', NULL, 'GAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522411, 'CTC', NULL, 'CTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522412, 'GTT', NULL, 'GTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522413, 'CTA', NULL, 'CTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522414, 'ATT', NULL, 'ATT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522415, 'CAT', NULL, 'CAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522416, 'TGG', NULL, 'TGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522417, 'GTC', NULL, 'GTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522418, 'TGC', NULL, 'TGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522419, 'TAA', NULL, 'TAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522420, 'CCG', NULL, 'CCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522421, 'GGA', NULL, 'GGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522422, 'ATC', NULL, 'ATC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522423, 'CGC', NULL, 'CGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522424, 'AGT', NULL, 'AGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522425, 'CAA', NULL, 'CAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522426, 'GCT', NULL, 'GCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522427, 'CGG', NULL, 'CGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522428, 'AAA', NULL, 'AAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522429, 'CTT', NULL, 'CTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522430, 'CGA', NULL, 'CGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522431, 'TAG', NULL, 'TAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522432, 'TTG', NULL, 'TTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522433, 'TCC', NULL, 'TCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522434, 'TTC', NULL, 'TTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522435, 'CCT', NULL, 'CCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522436, 'TGA', NULL, 'TGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522437, 'TCA', NULL, 'TCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522438, 'CAG', NULL, 'CAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522439, 'CAC', NULL, 'CAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522440, 'GCC', NULL, 'GCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522441, 'GGC', NULL, 'GGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522442, 'GCA', NULL, 'GCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522443, 'TGT', NULL, 'TGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522444, 'TCT', NULL, 'TCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522445, 'GGG', NULL, 'GGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522446, 'TAC', NULL, 'TAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522447, 'CCA', NULL, 'CCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522448, 'GTG', NULL, 'GTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522449, 'ACA', NULL, 'ACA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522450, 'CCC', NULL, 'CCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522451, 'AGA', NULL, 'AGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522452, 'ACT', NULL, 'ACT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522453, 'AAT', NULL, 'AAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522454, 'ATG', NULL, 'ATG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522455, 'GGT', NULL, 'GGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522456, 'AGC', NULL, 'AGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522457, 'CGT', NULL, 'CGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522458, 'Y', NULL, 'Tyrosine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522459, 'G', NULL, 'Glycine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522460, 'A', NULL, 'Alanine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522461, 'V', NULL, 'Valine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522462, 'I', NULL, 'Isoleucine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522463, 'L', NULL, 'Leucine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522464, 'P', NULL, 'Proline', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522465, 'S', NULL, 'Serine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522466, 'T', NULL, 'Threonine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522467, 'C', NULL, 'Cysteine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522468, 'M', NULL, 'Methionine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522469, 'D', NULL, 'Aspartic acid', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522470, 'N', NULL, 'Asparagine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522471, 'E', NULL, 'Glutamic acid', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522472, 'Q', NULL, 'Glutamine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522473, 'K', NULL, 'Lysine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522474, 'R', NULL, 'Arginine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522475, 'H', NULL, 'Histidine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522476, 'F', NULL, 'Phenylalanine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522477, 'W', NULL, 'Tryptophan', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54522478, 'X', NULL, 'Stop codon', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');

INSERT INTO public.element_key VALUES (54522389);
INSERT INTO public.element_key VALUES (54522390);
INSERT INTO public.element_key VALUES (54522391);
INSERT INTO public.element_key VALUES (54522392);
INSERT INTO public.element_key VALUES (54522393);
INSERT INTO public.element_key VALUES (54522394);
INSERT INTO public.element_key VALUES (54522395);
INSERT INTO public.element_key VALUES (54522396);
INSERT INTO public.element_key VALUES (54522397);
INSERT INTO public.element_key VALUES (54522398);
INSERT INTO public.element_key VALUES (54522399);
INSERT INTO public.element_key VALUES (54522400);
INSERT INTO public.element_key VALUES (54522401);
INSERT INTO public.element_key VALUES (54522402);
INSERT INTO public.element_key VALUES (54522403);
INSERT INTO public.element_key VALUES (54522404);
INSERT INTO public.element_key VALUES (54522405);
INSERT INTO public.element_key VALUES (54522406);
INSERT INTO public.element_key VALUES (54522407);
INSERT INTO public.element_key VALUES (54522408);
INSERT INTO public.element_key VALUES (54522409);
INSERT INTO public.element_key VALUES (54522410);
INSERT INTO public.element_key VALUES (54522411);
INSERT INTO public.element_key VALUES (54522412);
INSERT INTO public.element_key VALUES (54522413);
INSERT INTO public.element_key VALUES (54522414);
INSERT INTO public.element_key VALUES (54522415);
INSERT INTO public.element_key VALUES (54522416);
INSERT INTO public.element_key VALUES (54522417);
INSERT INTO public.element_key VALUES (54522418);
INSERT INTO public.element_key VALUES (54522419);
INSERT INTO public.element_key VALUES (54522420);
INSERT INTO public.element_key VALUES (54522421);
INSERT INTO public.element_key VALUES (54522422);
INSERT INTO public.element_key VALUES (54522423);
INSERT INTO public.element_key VALUES (54522424);
INSERT INTO public.element_key VALUES (54522425);
INSERT INTO public.element_key VALUES (54522426);
INSERT INTO public.element_key VALUES (54522427);
INSERT INTO public.element_key VALUES (54522428);
INSERT INTO public.element_key VALUES (54522429);
INSERT INTO public.element_key VALUES (54522430);
INSERT INTO public.element_key VALUES (54522431);
INSERT INTO public.element_key VALUES (54522432);
INSERT INTO public.element_key VALUES (54522433);
INSERT INTO public.element_key VALUES (54522434);
INSERT INTO public.element_key VALUES (54522435);
INSERT INTO public.element_key VALUES (54522436);
INSERT INTO public.element_key VALUES (54522437);
INSERT INTO public.element_key VALUES (54522438);
INSERT INTO public.element_key VALUES (54522439);
INSERT INTO public.element_key VALUES (54522440);
INSERT INTO public.element_key VALUES (54522441);
INSERT INTO public.element_key VALUES (54522442);
INSERT INTO public.element_key VALUES (54522443);
INSERT INTO public.element_key VALUES (54522444);
INSERT INTO public.element_key VALUES (54522445);
INSERT INTO public.element_key VALUES (54522446);
INSERT INTO public.element_key VALUES (54522447);
INSERT INTO public.element_key VALUES (54522448);
INSERT INTO public.element_key VALUES (54522449);
INSERT INTO public.element_key VALUES (54522450);
INSERT INTO public.element_key VALUES (54522451);
INSERT INTO public.element_key VALUES (54522452);
INSERT INTO public.element_key VALUES (54522453);
INSERT INTO public.element_key VALUES (54522454);
INSERT INTO public.element_key VALUES (54522455);
INSERT INTO public.element_key VALUES (54522456);
INSERT INTO public.element_key VALUES (54522457);
INSERT INTO public.element_key VALUES (54522458);
INSERT INTO public.element_key VALUES (54522459);
INSERT INTO public.element_key VALUES (54522460);
INSERT INTO public.element_key VALUES (54522461);
INSERT INTO public.element_key VALUES (54522462);
INSERT INTO public.element_key VALUES (54522463);
INSERT INTO public.element_key VALUES (54522464);
INSERT INTO public.element_key VALUES (54522465);
INSERT INTO public.element_key VALUES (54522466);
INSERT INTO public.element_key VALUES (54522467);
INSERT INTO public.element_key VALUES (54522468);
INSERT INTO public.element_key VALUES (54522469);
INSERT INTO public.element_key VALUES (54522470);
INSERT INTO public.element_key VALUES (54522471);
INSERT INTO public.element_key VALUES (54522472);
INSERT INTO public.element_key VALUES (54522473);
INSERT INTO public.element_key VALUES (54522474);
INSERT INTO public.element_key VALUES (54522475);
INSERT INTO public.element_key VALUES (54522476);
INSERT INTO public.element_key VALUES (54522477);
INSERT INTO public.element_key VALUES (54522478);

INSERT INTO public.full_characteristic_link VALUES (1, 1, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (2, 2, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (3, 2, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (4, 2, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (5, 2, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (6, 2, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (7, 3, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (8, 3, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (9, 3, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (10, 3, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (11, 3, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (12, 4, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (13, 4, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (14, 4, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (15, 4, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (16, 4, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (17, 5, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (18, 6, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (19, 6, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (20, 6, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (21, 6, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (22, 6, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (23, 7, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (24, 7, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (25, 7, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (26, 7, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (27, 7, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (28, 8, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (29, 8, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (30, 8, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (31, 8, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (32, 8, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (33, 9, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (34, 9, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (35, 9, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (36, 9, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (37, 9, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (38, 10, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (39, 10, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (40, 10, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (41, 10, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (42, 10, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (43, 11, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (44, 11, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (45, 11, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (46, 11, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (47, 11, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (48, 12, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (49, 12, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (50, 12, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (51, 12, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (52, 12, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (53, 13, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (54, 13, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (55, 13, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (56, 13, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (57, 13, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (58, 14, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (59, 14, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (60, 14, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (61, 14, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (62, 14, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (63, 15, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (64, 15, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (65, 15, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (66, 15, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (67, 15, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (68, 16, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (69, 16, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (70, 16, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (71, 16, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (72, 16, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (73, 17, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (74, 17, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (75, 17, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (76, 17, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (77, 17, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (78, 18, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (79, 18, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (80, 18, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (81, 18, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (82, 18, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (83, 19, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (84, 19, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (85, 19, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (86, 19, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (87, 19, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (88, 20, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (89, 21, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (90, 22, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (91, 23, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (92, 23, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (93, 23, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (94, 23, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (95, 23, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (96, 24, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (97, 24, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (98, 24, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (99, 24, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (100, 24, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (101, 25, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (102, 26, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (103, 26, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (104, 26, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (105, 26, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (106, 26, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (107, 27, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (108, 27, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (109, 27, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (110, 27, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (111, 27, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (112, 28, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (113, 28, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (114, 28, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (115, 28, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (116, 28, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (117, 29, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (118, 29, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (119, 29, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (120, 29, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (121, 29, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (122, 30, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (123, 30, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (124, 30, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (125, 30, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (126, 30, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (127, 31, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (128, 31, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (129, 31, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (130, 31, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (131, 31, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (132, 32, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (133, 33, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (134, 34, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (135, 35, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (136, 35, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (137, 35, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (138, 35, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (139, 35, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (140, 36, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (141, 36, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (142, 36, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (143, 36, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (144, 36, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (145, 37, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (146, 37, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (147, 37, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (148, 37, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (149, 37, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (150, 38, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (151, 38, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (152, 38, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (153, 38, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (154, 38, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (155, 39, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (156, 40, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (157, 41, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (158, 41, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (159, 41, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (160, 41, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (161, 41, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (162, 42, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (163, 43, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (164, 43, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (165, 43, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (166, 43, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (167, 43, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (168, 44, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (169, 44, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (170, 44, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (171, 44, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (172, 44, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (173, 45, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (174, 45, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (175, 45, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (176, 45, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (177, 45, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (178, 46, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (179, 46, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (180, 46, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (181, 46, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (182, 46, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (183, 47, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (184, 47, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (185, 47, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (186, 47, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (187, 47, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (188, 48, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (189, 48, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (190, 48, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (191, 48, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (192, 48, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (193, 49, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (194, 49, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (195, 49, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (196, 49, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (197, 49, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (198, 50, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (199, 51, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (200, 52, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (201, 52, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (202, 52, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (203, 52, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (204, 52, 5, 0);
INSERT INTO public.full_characteristic_link VALUES (205, 53, 0, 0);
INSERT INTO public.full_characteristic_link VALUES (206, 54, 1, 0);
INSERT INTO public.full_characteristic_link VALUES (207, 54, 2, 0);
INSERT INTO public.full_characteristic_link VALUES (208, 54, 3, 0);
INSERT INTO public.full_characteristic_link VALUES (209, 54, 4, 0);
INSERT INTO public.full_characteristic_link VALUES (210, 54, 5, 0);

SELECT pg_catalog.setval('public."AspNetPushNotificationSubscribers_Id_seq"', 20, false);

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);

SELECT pg_catalog.setval('public."AspNetRoles_Id_seq"', 1, false);

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 1, false);

SELECT pg_catalog.setval('public."AspNetUsers_Id_seq"', 57, false);

SELECT pg_catalog.setval('public.accordance_characteristic_id_seq', 1, false);

SELECT pg_catalog.setval('public.accordance_characteristic_link_id_seq', 8, true);

SELECT pg_catalog.setval('public.binary_characteristic_id_seq', 14612, true);

SELECT pg_catalog.setval('public.binary_characteristic_link_id_seq', 18, true);

SELECT pg_catalog.setval('public.chain_attribute_id_seq', 123377035, true);

SELECT pg_catalog.setval('public.characteristics_id_seq', 154144744, true);

SELECT pg_catalog.setval('public.congeneric_characteristic_id_seq', 6280, true);

SELECT pg_catalog.setval('public.congeneric_characteristic_link_id_seq', 102, true);

SELECT pg_catalog.setval('public.elements_id_seq', 54522478, true);

SELECT pg_catalog.setval('public.full_characteristic_link_id_seq', 215, true);

SELECT pg_catalog.setval('public.matter_id_seq', 18974, true);

SELECT pg_catalog.setval('public.multisequence_id_seq', 6, true);

SELECT pg_catalog.setval('public.piece_id_seq', 146620, true);

SELECT pg_catalog.setval('public.pitch_id_seq', 682, true);

SELECT pg_catalog.setval('public.sequence_group_id_seq', 2, true);

SELECT pg_catalog.setval('public.task_id_seq', 1080, true);

SELECT pg_catalog.setval('public.task_result_id_seq', 397, true);

ALTER TABLE ONLY public."AspNetPushNotificationSubscribers"
    ADD CONSTRAINT "PK_AspNetPushNotificationSubscribers" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");

ALTER TABLE ONLY public.data_chain
    ADD CONSTRAINT data_chain_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.multisequence
    ADD CONSTRAINT multisequence_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT pk_accordance_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.accordance_characteristic_link
    ADD CONSTRAINT pk_accordance_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT pk_binary_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.binary_characteristic_link
    ADD CONSTRAINT pk_binary_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.chain
    ADD CONSTRAINT pk_chain PRIMARY KEY (id);

ALTER TABLE ONLY public.chain_attribute
    ADD CONSTRAINT pk_chain_attribute PRIMARY KEY (id);

ALTER TABLE ONLY public.chain_key
    ADD CONSTRAINT pk_chain_key PRIMARY KEY (id);

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT pk_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT pk_congeneric_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.congeneric_characteristic_link
    ADD CONSTRAINT pk_congeneric_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.dna_chain
    ADD CONSTRAINT pk_dna_chain PRIMARY KEY (id);

ALTER TABLE ONLY public.element_key
    ADD CONSTRAINT pk_element_key PRIMARY KEY (id);

ALTER TABLE ONLY public.element
    ADD CONSTRAINT pk_elements PRIMARY KEY (id);

ALTER TABLE ONLY public.fmotif
    ADD CONSTRAINT pk_fmotif PRIMARY KEY (id);

ALTER TABLE ONLY public.full_characteristic_link
    ADD CONSTRAINT pk_full_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT pk_image_sequence PRIMARY KEY (id);

ALTER TABLE ONLY public.literature_chain
    ADD CONSTRAINT pk_literature_chain PRIMARY KEY (id);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT pk_matter PRIMARY KEY (id);

ALTER TABLE ONLY public.measure
    ADD CONSTRAINT pk_measure PRIMARY KEY (id);

ALTER TABLE ONLY public.music_chain
    ADD CONSTRAINT pk_music_chain PRIMARY KEY (id);

ALTER TABLE ONLY public.note
    ADD CONSTRAINT pk_note PRIMARY KEY (id);

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT pk_note_pitch PRIMARY KEY (note_id, pitch_id);

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT pk_piece PRIMARY KEY (id);

ALTER TABLE ONLY public.pitch
    ADD CONSTRAINT pk_pitch PRIMARY KEY (id);

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT pk_subsequence PRIMARY KEY (id);

ALTER TABLE ONLY public.task
    ADD CONSTRAINT pk_task PRIMARY KEY (id);

ALTER TABLE ONLY public.sequence_group_matter
    ADD CONSTRAINT sequence_group_matter_pkey PRIMARY KEY (matter_id, group_id);

ALTER TABLE ONLY public.sequence_group
    ADD CONSTRAINT sequence_group_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.task_result
    ADD CONSTRAINT task_result_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public."AspNetPushNotificationSubscribers"
    ADD CONSTRAINT "uk_AspNetPushNotificationSubscribers" UNIQUE ("UserId", "Endpoint");

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT uk_accordance_characteristic UNIQUE (first_chain_id, second_chain_id, first_element_id, second_element_id, characteristic_link_id);

ALTER TABLE ONLY public.accordance_characteristic_link
    ADD CONSTRAINT uk_accordance_characteristic_link UNIQUE (accordance_characteristic, link);

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT uk_binary_characteristic UNIQUE (chain_id, first_element_id, second_element_id, characteristic_link_id);

ALTER TABLE ONLY public.binary_characteristic_link
    ADD CONSTRAINT uk_binary_characteristic_link UNIQUE (binary_characteristic, link);

ALTER TABLE ONLY public.chain_attribute
    ADD CONSTRAINT uk_chain_attribute UNIQUE (chain_id, attribute, value);

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT uk_characteristic UNIQUE (chain_id, characteristic_link_id);

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT uk_congeneric_characteristic UNIQUE (chain_id, element_id, characteristic_link_id);

ALTER TABLE ONLY public.congeneric_characteristic_link
    ADD CONSTRAINT uk_congeneric_characteristic_link UNIQUE (congeneric_characteristic, link, arrangement_type);

ALTER TABLE ONLY public.data_chain
    ADD CONSTRAINT uk_data_chain UNIQUE (notation, matter_id);

ALTER TABLE ONLY public.dna_chain
    ADD CONSTRAINT uk_dna_chain UNIQUE (matter_id, notation);

ALTER TABLE ONLY public.element
    ADD CONSTRAINT uk_element_value_notation UNIQUE (value, notation);

ALTER TABLE ONLY public.full_characteristic_link
    ADD CONSTRAINT uk_full_characteristic_link UNIQUE (full_characteristic, link, arrangement_type);

ALTER TABLE ONLY public.literature_chain
    ADD CONSTRAINT uk_literature_chain UNIQUE (notation, matter_id, language, translator);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT uk_matter UNIQUE (name, nature);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT uk_matter_multisequence UNIQUE (multisequence_id, multisequence_number) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.multisequence
    ADD CONSTRAINT uk_multisequence_name UNIQUE (name);

ALTER TABLE ONLY public.music_chain
    ADD CONSTRAINT uk_music_chain UNIQUE (matter_id, notation, pause_treatment, sequential_transfer);

ALTER TABLE ONLY public.note
    ADD CONSTRAINT uk_note UNIQUE (value);

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT uk_piece UNIQUE (subsequence_id, start, length);

ALTER TABLE ONLY public.pitch
    ADD CONSTRAINT uk_pitch UNIQUE (octave, instrument, accidental, note_symbol);

ALTER TABLE ONLY public.sequence_group
    ADD CONSTRAINT uk_sequence_group_name UNIQUE (name);

ALTER TABLE ONLY public.task_result
    ADD CONSTRAINT uk_task_result UNIQUE (task_id, key);

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");

CREATE INDEX "IX_AspNetPushNotificationSubscribers_UserId" ON public."AspNetPushNotificationSubscribers" USING btree ("UserId");

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");

CREATE INDEX data_chain_alphabet_idx ON public.data_chain USING gin (alphabet);

CREATE INDEX data_chain_matter_id_idx ON public.data_chain USING btree (matter_id);

COMMENT ON INDEX public.data_chain_matter_id_idx IS 'Индекс по объектам исследования которым принадлежат цепочки.';

CREATE INDEX data_chain_notation_id_idx ON public.data_chain USING btree (notation);

COMMENT ON INDEX public.data_chain_notation_id_idx IS 'Индекс по формам записи цепочек.';

CREATE INDEX fki_congeneric_characteristic_alphabet_element ON public.congeneric_characteristic USING btree (chain_id, element_id);

CREATE INDEX ix_accordance_characteristic_first_chain_id ON public.accordance_characteristic USING btree (first_chain_id);

COMMENT ON INDEX public.ix_accordance_characteristic_first_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_accordance_characteristic_first_sequence_id_brin ON public.accordance_characteristic USING brin (first_chain_id);

CREATE INDEX ix_accordance_characteristic_second_chain_id ON public.accordance_characteristic USING btree (second_chain_id);

COMMENT ON INDEX public.ix_accordance_characteristic_second_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_accordance_characteristic_second_sequence_id_brin ON public.accordance_characteristic USING brin (second_chain_id);

CREATE INDEX ix_accordance_characteristic_sequences_ids_brin ON public.accordance_characteristic USING brin (first_chain_id, second_chain_id);

CREATE INDEX ix_accordance_characteristic_sequences_ids_characteristic_link_ ON public.accordance_characteristic USING brin (first_chain_id, second_chain_id, characteristic_link_id);

CREATE INDEX ix_binary_characteristic_chain_id ON public.binary_characteristic USING btree (chain_id);

COMMENT ON INDEX public.ix_binary_characteristic_chain_id IS 'Индекс бинарных характеристик по цепочкам.';

CREATE INDEX ix_binary_characteristic_first_sequence_id_brin ON public.binary_characteristic USING brin (chain_id);

CREATE INDEX ix_binary_characteristic_sequence_id_characteristic_link_id_bri ON public.binary_characteristic USING brin (chain_id, characteristic_link_id);

CREATE INDEX ix_chain_alphabet ON public.chain USING gin (alphabet);

CREATE INDEX ix_chain_matter_id ON public.chain USING btree (matter_id);

COMMENT ON INDEX public.ix_chain_matter_id IS 'Индекс по объектам исследования которым принадлежат цепочки.';

CREATE INDEX ix_chain_notation_id ON public.chain USING btree (notation);

COMMENT ON INDEX public.ix_chain_notation_id IS 'Индекс по формам записи цепочек.';

CREATE INDEX ix_characteristic_chain_id ON public.full_characteristic USING btree (chain_id);

COMMENT ON INDEX public.ix_characteristic_chain_id IS 'Индекс характерисктик по цепочкам.';

CREATE INDEX ix_characteristic_characteristic_type_link ON public.full_characteristic USING btree (characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_chain_id ON public.congeneric_characteristic USING btree (chain_id);

CREATE INDEX ix_congeneric_characteristic_characteristic_link_id_brin ON public.congeneric_characteristic USING brin (characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_brin ON public.congeneric_characteristic USING brin (chain_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_characteristic_link_id ON public.congeneric_characteristic USING brin (chain_id, characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_element_id_brin ON public.congeneric_characteristic USING brin (chain_id, element_id);

CREATE INDEX ix_dna_chain_alphabet ON public.dna_chain USING gin (alphabet);

CREATE INDEX ix_dna_chain_matter_id ON public.dna_chain USING btree (matter_id);

COMMENT ON INDEX public.ix_dna_chain_matter_id IS 'Индекс по объектам исследования которым принадлежат цепчоки ДНК.';

CREATE INDEX ix_dna_chain_notation_id ON public.dna_chain USING btree (notation);

COMMENT ON INDEX public.ix_dna_chain_notation_id IS 'Индекс по форме записи цепочек ДНК.';

CREATE INDEX ix_element_notation_id ON public.element USING btree (notation);

COMMENT ON INDEX public.ix_element_notation_id IS 'Индекс элементов по форме записи.';

CREATE INDEX ix_element_value ON public.element USING btree (value);

COMMENT ON INDEX public.ix_element_value IS 'Index in value of element.';

CREATE INDEX ix_fmotif_alphabet ON public.fmotif USING gin (alphabet);

CREATE INDEX ix_full_characteristic_characteristic_link_id_brin ON public.full_characteristic USING brin (characteristic_link_id);

CREATE INDEX ix_full_characteristic_sequence_id_brin ON public.full_characteristic USING brin (chain_id);

CREATE INDEX ix_image_sequence_matter_id ON public.image_sequence USING btree (matter_id);

CREATE INDEX ix_literature_chain_alphabet ON public.literature_chain USING gin (alphabet);

CREATE INDEX ix_literature_chain_matter_id ON public.literature_chain USING btree (matter_id);

CREATE INDEX ix_literature_chain_matter_language ON public.literature_chain USING btree (matter_id, language);

CREATE INDEX ix_literature_chain_notation_id ON public.literature_chain USING btree (notation);

CREATE INDEX ix_matter_nature ON public.matter USING btree (nature);

COMMENT ON INDEX public.ix_matter_nature IS 'Индекс по природе таблицы matter.';

CREATE INDEX ix_measure_alphabet ON public.measure USING gin (alphabet);

CREATE INDEX ix_measure_notation_id ON public.measure USING btree (notation);

CREATE INDEX ix_music_chain_alphabet ON public.music_chain USING gin (alphabet);

CREATE INDEX ix_music_chain_matter_id ON public.music_chain USING btree (matter_id);

CREATE INDEX ix_music_chain_notation_id ON public.music_chain USING btree (notation);

CREATE INDEX ix_note_notation_id ON public.note USING btree (notation);

CREATE INDEX ix_position_subsequence_id ON public."position" USING btree (subsequence_id);

CREATE INDEX ix_position_subsequence_id_brin ON public."position" USING brin (subsequence_id);

CREATE INDEX ix_sequence_attribute_sequence_id_brin ON public.chain_attribute USING brin (chain_id);

CREATE INDEX ix_subsequence_chain_feature ON public.subsequence USING btree (chain_id, feature);

CREATE INDEX ix_subsequence_chain_id ON public.subsequence USING btree (chain_id);

CREATE INDEX ix_subsequence_feature_brin ON public.subsequence USING brin (feature);

CREATE INDEX ix_subsequence_sequence_id_brin ON public.subsequence USING brin (chain_id);

CREATE INDEX ix_subsequence_sequence_id_feature_brin ON public.subsequence USING brin (chain_id, feature);

CREATE TRIGGER tgd_element_key BEFORE DELETE ON public.element_key FOR EACH ROW EXECUTE FUNCTION public.trigger_element_delete_alphabet_bound();

COMMENT ON TRIGGER tgd_element_key ON public.element_key IS 'Проверяет, не используется ли удаляемый элемент в каком-либо алфавите.';

CREATE TRIGGER tgi_chain_key BEFORE INSERT ON public.chain_key FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_unique_check();

CREATE TRIGGER tgi_element_key BEFORE INSERT ON public.element_key FOR EACH ROW EXECUTE FUNCTION public.trigger_element_key_insert();

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabet BEFORE INSERT OR UPDATE OF first_element_id, second_element_id, chain_id ON public.binary_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_elements_in_alphabet();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabet ON public.binary_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabets BEFORE INSERT OR UPDATE OF first_chain_id, second_chain_id, first_element_id, second_element_id ON public.accordance_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_elements_in_alphabets();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabets ON public.accordance_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.chain FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_chain_alphabet_check ON public.chain IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_chain_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.chain FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_chain_building_check ON public.chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_chain_modified BEFORE INSERT OR UPDATE ON public.chain FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_chain_modified ON public.chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet BEFORE INSERT OR UPDATE OF element_id, chain_id ON public.congeneric_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_element_in_alphabet();

COMMENT ON TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet ON public.congeneric_characteristic IS 'Триггер, проверяющий что элемент однородной цепочки, для которой вычислена характеристика, присутствует в алфавите данной цепочки.';

CREATE TRIGGER tgiu_data_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.data_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_data_chain_alphabet_check ON public.data_chain IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_data_chain_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.data_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_data_chain_building_check ON public.data_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_data_chain_modified BEFORE INSERT OR UPDATE ON public.data_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_data_chain_modified ON public.data_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_dna_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.dna_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_dna_chain_alphabet_check ON public.dna_chain IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_dna_chain_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.dna_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_dna_chain_building_check ON public.dna_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_dna_chain_modified BEFORE INSERT OR UPDATE ON public.dna_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_dna_chain_modified ON public.dna_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_element_modified BEFORE INSERT OR UPDATE ON public.element FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_element_modified ON public.element IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_fmotif_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_check_notes_alphabet();

COMMENT ON TRIGGER tgiu_fmotif_alphabet_check ON public.fmotif IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_fmotif_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_fmotif_building_check ON public.fmotif IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_fmotif_modified BEFORE INSERT OR UPDATE ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_fmotif_modified ON public.fmotif IS 'Trigger filling creation and modification date';

CREATE TRIGGER tgiu_image_sequence_modified BEFORE INSERT OR UPDATE ON public.image_sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

CREATE TRIGGER tgiu_literature_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.literature_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_literature_chain_alphabet_check ON public.literature_chain IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_literature_chain_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.literature_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_literature_chain_building_check ON public.literature_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_literature_chain_modified BEFORE INSERT OR UPDATE ON public.literature_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_literature_chain_modified ON public.literature_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_matter_modified BEFORE INSERT OR UPDATE ON public.matter FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_matter_modified ON public.matter IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_measure_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_check_notes_alphabet();

COMMENT ON TRIGGER tgiu_measure_alphabet_check ON public.measure IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_measure_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_measure_building_check ON public.measure IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_measure_modified BEFORE INSERT OR UPDATE ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_measure_modified ON public.measure IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_music_chain_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.music_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_music_chain_alphabet_check ON public.music_chain IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_music_chain_building_check BEFORE INSERT OR UPDATE OF alphabet, building ON public.music_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_building_check();

COMMENT ON TRIGGER tgiu_music_chain_building_check ON public.music_chain IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_music_chain_modified BEFORE INSERT OR UPDATE ON public.music_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_music_chain_modified ON public.music_chain IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_note_modified BEFORE INSERT OR UPDATE ON public.note FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_note_modified ON public.note IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_sequence_group_modified BEFORE INSERT OR UPDATE ON public.sequence_group FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_sequence_group_modified ON public.sequence_group IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_subsequence_modified BEFORE INSERT OR UPDATE ON public.subsequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_subsequence_modified ON public.subsequence IS 'Trigger adding creation and modification dates.';

CREATE TRIGGER tgiud_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.chain FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_chain_chain_key_bound ON public.chain IS 'Дублирует добавление, изменение и удаление записей в таблице chain в таблицу chain_key.';

CREATE TRIGGER tgiud_data_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.data_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_data_chain_chain_key_bound ON public.data_chain IS 'Дублирует добавление, изменение и удаление записей в таблице chain в таблицу chain_key.';

CREATE TRIGGER tgiud_dna_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.dna_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_dna_chain_chain_key_bound ON public.dna_chain IS 'Дублирует добавление, изменение и удаление записей в таблице dna_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_element_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.element FOR EACH ROW EXECUTE FUNCTION public.trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_element_element_key_bound ON public.element IS 'Дублирует добавление, изменение и удаление записей в таблице element в таблицу element_key.';

CREATE TRIGGER tgiud_fmotif_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_fmotif_element_key_bound ON public.fmotif IS 'Trigger that dublicates insert, update and delete of fmotifs into element_key table';

CREATE TRIGGER tgiud_image_sequence_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.image_sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

CREATE TRIGGER tgiud_literature_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.literature_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_literature_chain_chain_key_bound ON public.literature_chain IS 'Дублирует добавление, изменение и удаление записей в таблице literature_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_measure_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_measure_element_key_bound ON public.measure IS 'Дублирует добавление, изменение и удаление записей в таблице measure в таблицу element_key.';

CREATE TRIGGER tgiud_music_chain_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.music_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_music_chain_chain_key_bound ON public.music_chain IS 'Дублирует добавление, изменение и удаление записей в таблице music_chain в таблицу chain_key.';

CREATE TRIGGER tgiud_note_element_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.note FOR EACH ROW EXECUTE FUNCTION public.trigger_element_key_bound();

COMMENT ON TRIGGER tgiud_note_element_key_bound ON public.note IS 'Дублирует добавление, изменение и удаление записей в таблице note в таблицу element_key.';

CREATE TRIGGER tgiud_subsequence_chain_key_bound AFTER INSERT OR DELETE OR UPDATE OF id ON public.subsequence FOR EACH ROW EXECUTE FUNCTION public.trigger_chain_key_bound();

COMMENT ON TRIGGER tgiud_subsequence_chain_key_bound ON public.subsequence IS 'Creates two way bound with chain_key table.';

CREATE TRIGGER tgu_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON public.chain FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_chain_characteristics_delete ON public.chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_data_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON public.data_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_data_chain_characteristics_delete ON public.data_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_dna_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON public.dna_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_dna_chain_characteristics_delete ON public.dna_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_element_key AFTER UPDATE ON public.element_key FOR EACH ROW EXECUTE FUNCTION public.trigger_element_update_alphabet();

COMMENT ON TRIGGER tgu_element_key ON public.element_key IS 'Триггер обновляющие все зависи в алфавтиах цепочек при перенумеровке элементов. Теоретически работает очень медленно, особенно при массовом перенумеровании.';

CREATE TRIGGER tgu_literature_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON public.literature_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_literature_chain_characteristics_delete ON public.literature_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_measure_characteristics AFTER UPDATE ON public.measure FOR EACH STATEMENT EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_measure_characteristics ON public.measure IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_music_chain_characteristics_delete AFTER UPDATE OF alphabet, building ON public.music_chain FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_music_chain_characteristics_delete ON public.music_chain IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_subsequence_characteristics_delete AFTER UPDATE OF start, length, chain_id ON public.subsequence FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_chain_characteristics();

COMMENT ON TRIGGER tgu_subsequence_characteristics_delete ON public.subsequence IS 'Trigger deleting all characteristics of subsequences that has been updated.';

ALTER TABLE ONLY public."AspNetPushNotificationSubscribers"
    ADD CONSTRAINT "FK_AspNetPushNotificationSubscribers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_element_key_first FOREIGN KEY (first_element_id) REFERENCES public.element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_element_key_second FOREIGN KEY (second_element_id) REFERENCES public.element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_first_chain_key FOREIGN KEY (first_chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.accordance_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_second_chain_key FOREIGN KEY (second_chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_chain_key FOREIGN KEY (chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_key_first FOREIGN KEY (first_element_id) REFERENCES public.element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_key_second FOREIGN KEY (second_element_id) REFERENCES public.element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.binary_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.chain_attribute
    ADD CONSTRAINT fk_chain_attribute_chain FOREIGN KEY (chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.chain
    ADD CONSTRAINT fk_chain_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.chain
    ADD CONSTRAINT fk_chain_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT fk_characterisric_chain_key FOREIGN KEY (chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_chain_key FOREIGN KEY (chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_element_key FOREIGN KEY (element_id) REFERENCES public.element_key(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.congeneric_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.data_chain
    ADD CONSTRAINT fk_data_chain_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.data_chain
    ADD CONSTRAINT fk_data_chain_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.dna_chain
    ADD CONSTRAINT fk_dna_chain_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.dna_chain
    ADD CONSTRAINT fk_dna_chain_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.element
    ADD CONSTRAINT fk_element_element_key FOREIGN KEY (id) REFERENCES public.element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.fmotif
    ADD CONSTRAINT fk_fmotif_element_key FOREIGN KEY (id) REFERENCES public.element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT fk_full_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.full_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT fk_image_sequence_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT fk_image_sequence_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.literature_chain
    ADD CONSTRAINT fk_literature_chain_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.literature_chain
    ADD CONSTRAINT fk_literature_chain_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT fk_matter_multisequence FOREIGN KEY (multisequence_id) REFERENCES public.multisequence(id) ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE ONLY public.sequence_group_matter
    ADD CONSTRAINT fk_matter_sequence_group FOREIGN KEY (group_id) REFERENCES public.sequence_group(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.measure
    ADD CONSTRAINT fk_measure_element_key FOREIGN KEY (id) REFERENCES public.element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.music_chain
    ADD CONSTRAINT fk_music_chain_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.music_chain
    ADD CONSTRAINT fk_music_chain_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.note
    ADD CONSTRAINT fk_note_element_key FOREIGN KEY (id) REFERENCES public.element_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT fk_note_pitch_note FOREIGN KEY (note_id) REFERENCES public.note(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT fk_note_pitch_pitch FOREIGN KEY (pitch_id) REFERENCES public.pitch(id) ON UPDATE RESTRICT ON DELETE RESTRICT;

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT fk_position_subsequence FOREIGN KEY (subsequence_id) REFERENCES public.subsequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.sequence_group_matter
    ADD CONSTRAINT fk_sequence_group_matter FOREIGN KEY (matter_id) REFERENCES public.matter(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT fk_subsequence_chain_chain_key FOREIGN KEY (chain_id) REFERENCES public.chain_key(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT fk_subsequence_chain_key FOREIGN KEY (id) REFERENCES public.chain_key(id) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.task_result
    ADD CONSTRAINT fk_task_result_task FOREIGN KEY (task_id) REFERENCES public.task(id) ON UPDATE CASCADE ON DELETE CASCADE;
	
COMMIT;

--23.01.2024 18:32:53
