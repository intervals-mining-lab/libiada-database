--20.01.2025 14:33:09
BEGIN;

SET transaction_timeout = 0;

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';

CREATE FUNCTION public.check_element_in_alphabet(sequence_id bigint, element_id bigint) RETURNS boolean
    LANGUAGE sql STABLE
    AS $$
SELECT count(*) = 1 result
FROM (SELECT alphabet a
      FROM sequence
      WHERE id = sequence_id) s
WHERE element_id = ANY (s.a);
$$;

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
abstract_sequences_disproportion integer;
elements_ids_count integer;
distinct_elements_count integer;
element_disproportion integer;
orphaned_elements_count integer;
orphaned_congeneric_characteristics integer;
orphaned_binary_characteristics integer;
orphaned_accordance_characteristics integer;
BEGIN
RAISE INFO 'Checking referential integrity of the database.';

RAISE INFO 'Checking sequence table and its children.';

SELECT INTO sequence_ids_count, distincs_sequence_ids_count
    COUNT(s.id) , COUNT(DISTINCT s.id) 
    FROM (SELECT id FROM sequence 
         UNION ALL SELECT id FROM subsequence 
         UNION ALL SELECT id FROM image_sequence) s;
    
IF sequence_ids_count != distincs_sequence_ids_count THEN
    RAISE EXCEPTION  'Ids in sequence table and/or its cildren are not unique.';
ELSE
    RAISE INFO 'All sequence ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in sequence table and its children to the records in abstract_sequence table.';

SELECT INTO abstract_sequences_disproportion COUNT(*)  
    FROM (SELECT id FROM sequence 
         UNION ALL SELECT id FROM subsequence 
         UNION ALL SELECT id FROM image_sequence) s 
    FULL OUTER JOIN abstract_sequence a ON a.id = s.id 
    WHERE s.id IS NULL OR a.id IS NULL;

IF abstract_sequences_disproportion > 0 THEN
    RAISE EXCEPTION 'Number of records in abstract_sequence is not equal to number of records in sequence table and its children.';
ELSE
    RAISE INFO 'abstract_sequence is in sync with sequence table and its children.';
END IF;

RAISE INFO 'Sequences tables are all checked.';

RAISE INFO 'Checking element table and its children.';

SELECT INTO elements_ids_count, distinct_elements_count
    COUNT(id), COUNT(DISTINCT id) 
    FROM (SELECT id FROM fmotif 
         UNION ALL SELECT id FROM measure 
         UNION ALL SELECT id FROM note) u;
         
IF elements_ids_count != distinct_elements_count THEN
    RAISE EXCEPTION 'ids in element cildren tables are not unique.';
ELSE
    RAISE INFO 'All element ids are unique.';
END IF;

RAISE INFO 'Checking accordance of records in element table and its children tables.';

SELECT INTO element_disproportion COUNT (*)
    FROM (SELECT f.id, e.id el_id FROM fmotif f
         FULL OUTER JOIN element e
         ON f.id = e.id 
         WHERE e.notation = 6 -- fmotifs notation
         UNION ALL 
         SELECT m.id, e.id el_id FROM measure m
         FULL OUTER JOIN element e
         ON m.id = e.id 
         WHERE e.notation = 7 -- measures notation
         UNION ALL 
         SELECT n.id, e.id el_id FROM note n
         FULL OUTER JOIN element e
         ON n.id = e.id 
         WHERE e.notation = 8) u -- notes notation
     WHERE u.id IS NULL OR u.el_id IS NULL;

IF element_disproportion > 0 THEN
    RAISE EXCEPTION 'Number of records in element is not equal to number of records in element and its children.';
ELSE
    RAISE INFO 'element is in sync with element and its children.';
END IF;

RAISE INFO 'Elements tables are all checked.';

RAISE INFO 'Checking alphabets of all sequences.';

SELECT INTO orphaned_elements_count COUNT(c.a)
    FROM (SELECT unnest(alphabet) a FROM sequence 
         UNION SELECT unnest(alphabet) a FROM measure
         UNION SELECT unnest(alphabet) a FROM fmotif) c 
          LEFT OUTER JOIN element e 
          ON e.id = c.a 
          WHERE e.id IS NULL;

IF orphaned_elements_count > 0 THEN 
    RAISE EXCEPTION 'There are % missing elements of alphabet.', orphaned_elements_count;
ELSE
    RAISE INFO 'All alphabets elements are present in element table.';
END IF;

SELECT INTO orphaned_congeneric_characteristics COUNT(cc.id)
    FROM congeneric_characteristic cc 
    LEFT OUTER JOIN (SELECT unnest(alphabet) a, id FROM sequence) c 
    ON c.id = cc.sequence_id AND c.a = cc.element_id 
    WHERE c.a IS NULL;

IF orphaned_congeneric_characteristics > 0 THEN
    RAISE EXCEPTION 'There are % orphaned congeneric characteristics without according elements in alphabets.', orphaned_congeneric_characteristics;
ELSE
    RAISE INFO 'All congeneric characteristics have corresponding elements in alphabets.';
END IF;

SELECT INTO orphaned_binary_characteristics COUNT(bc.id)
    FROM binary_characteristic bc 
    LEFT OUTER JOIN 
        (SELECT unnest(alphabet) a, id FROM sequence) c 
    ON c.id = bc.sequence_id AND (c.a = bc.first_element_id OR c.a = bc.second_element_id)
    WHERE c.a IS NULL;

IF orphaned_binary_characteristics > 0 THEN
    RAISE EXCEPTION 'There are % orphaned binary characteristics without according elements in alphabets.', orphaned_binary_characteristics;
ELSE
    RAISE INFO 'All binary characteristics have corresponding elements in alphabets.';
END IF;

SELECT INTO orphaned_accordance_characteristics COUNT(ac.id)
    FROM accordance_characteristic ac 
    LEFT OUTER JOIN (SELECT unnest(alphabet) a, id FROM sequence) c 
    ON (c.id = ac.first_sequence_id AND c.a = ac.first_element_id) OR (c.id = ac.second_sequence_id AND c.a = ac.second_element_id)
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
        
        SELECT count(1) INTO orphaned_elements result FROM unnest(NEW.alphabet) a LEFT OUTER JOIN element e ON e.id = a WHERE e.id IS NULL;
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
    element_in_alphabet := check_element_in_alphabet(NEW.sequence_id, NEW.element_id);
    IF element_in_alphabet THEN
        RETURN NEW;
    ELSE 
        RAISE EXCEPTION 'New characteristic is referencing element (id = %) not present in sequence (id = %) alphabet.', NEW.element_id, NEW.sequence_id;
    END IF;
ELSE
    RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with sequence_id, element_id.';
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
    first_element_in_alphabet := check_element_in_alphabet(NEW.sequence_id, NEW.first_element_id);
    second_element_in_alphabet := check_element_in_alphabet(NEW.sequence_id, NEW.second_element_id);
    IF first_element_in_alphabet AND second_element_in_alphabet THEN
        RETURN NEW;
    ELSE 
        RAISE EXCEPTION 'New characteristic is referencing element or elements (first_id = %, second_id = %) not present in sequence (id = %) alphabet.', NEW.first_element_id, NEW.second_element_id, NEW.sequence_id;
    END IF;
ELSE
    RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with sequence_id, first_element_id, second_element_id.';
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
    first_element_in_alphabet := check_element_in_alphabet(NEW.first_sequence_id, NEW.first_element_id);
    second_element_in_alphabet := check_element_in_alphabet(NEW.second_sequence_id, NEW.second_element_id);
    IF first_element_in_alphabet AND second_element_in_alphabet THEN
        RETURN NEW;
    ELSE 
        RAISE EXCEPTION 'New characteristic is referencing element or elements (first_id = %, second_id = %) not present in sequences (first_sequence_id = %, second_sequence_id = %) alphabet.', NEW.first_element_id, NEW.second_element_id, NEW.first_sequence_id, NEW.second_sequence_id;
    END IF;
ELSE
    RAISE EXCEPTION 'Unknown operation. This trigger shoud be used only in insert and update operation on tables with first_sequence_id, second_sequence_id, first_element_id, second_element_id.';
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

CREATE FUNCTION public.trigger_check_order() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
max_value integer;
BEGIN
IF TG_OP = 'UPDATE' AND array_length(OLD.alphabet, 1) = array_length(NEW.alphabet, 1) AND NEW.order = OLD.order THEN
    RETURN NEW;
END IF;
IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
    IF NEW.order[1] != 1 THEN
        RAISE EXCEPTION 'First order value is not 1. Actual value %.', NEW.order[1];
    END IF;
    max_value := 0;
    FOR i IN array_lower(NEW.order, 1)..array_upper(NEW.order, 1) LOOP
        IF NEW.order[i] > (max_value + 1) THEN
            RAISE EXCEPTION 'Order is incorrect starting from % position.', i ;
        END IF;
        IF NEW.order[i] = (max_value + 1) THEN
            max_value := NEW.order[i];
        END IF;
    END LOOP;
    IF max_value != array_length(NEW.alphabet, 1) THEN
        RAISE EXCEPTION 'Alphabet size is not equal to the order maximum value. Alphabet elements count %, and order max value %.', array_length(NEW.alphabet, 1), max_value;
    END IF;
    RETURN NEW;
ELSE
    RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT or UPDATE operation on tables with order column.';
END IF;
END
$$;

COMMENT ON FUNCTION public.trigger_check_order() IS 'Validates inner consistency of the order of given sequence.';

CREATE FUNCTION public.trigger_delete_sequence_characteristics() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'UPDATE' THEN
    DELETE FROM full_characteristic WHERE full_characteristic.sequence_id = OLD.id;
    DELETE FROM binary_characteristic WHERE binary_characteristic.sequence_id = OLD.id;
    DELETE FROM congeneric_characteristic WHERE congeneric_characteristic.sequence_id = OLD.id;
    DELETE FROM accordance_characteristic WHERE accordance_characteristic.first_sequence_id = OLD.id OR accordance_characteristic.second_sequence_id = OLD.id;
    RETURN NEW;
ELSE
    RAISE EXCEPTION 'Unknown operation. This trigger only works on UPDATE operation.';
END IF;
END;
$$;

COMMENT ON FUNCTION public.trigger_delete_sequence_characteristics() IS 'Trigger function deleting all characteristics of sequences that has been updated.';

CREATE FUNCTION public.trigger_element_delete_alphabet_bound() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
element_used bool;
BEGIN
IF TG_OP = 'DELETE' THEN
    SELECT count(*) > 0 INTO element_used 
    FROM (SELECT DISTINCT unnest(alphabet) a 
          FROM (SELECT alphabet FROM sequence 
                UNION SELECT alphabet FROM fmotif 
                UNION SELECT alphabet FROM measure) c
          WHERE OLD.id = ANY (c.alphabet)) s;
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

CREATE FUNCTION public.trigger_element_update_alphabet() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_OP = 'UPDATE' THEN
    UPDATE sequence SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM sequence c1 WHERE OLD.id = ANY (alphabet)) c1 WHERE sequnce.id = c1.id;
    UPDATE fmotif SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM fmotif c1 WHERE OLD.id = ANY (alphabet)) c1 WHERE fmotif.id = c1.id;
    UPDATE measure SET alphabet = c1.alphabet FROM (SELECT c1.id, array_replace(c1.alphabet, OLD.id, NEW.id) alphabet FROM measure c1 WHERE OLD.id = ANY (alphabet)) c1 WHERE measure.id = c1.id;
    RETURN NEW;
END IF; 
RAISE EXCEPTION 'Unknown operation. This trigger is only meat for update operations on tables with alphabet field';
END
$$;

COMMENT ON FUNCTION public.trigger_element_update_alphabet() IS 'Automaticly updates elements ids in sequences alphabet when ids are changed in element table.';

CREATE FUNCTION public.trigger_set_abstract_sequence_modified() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_TABLE_NAME = 'abstract_sequence' THEN
    NEW.modified := now();
    IF TG_OP = 'INSERT' THEN
        NEW.created := now();
        RETURN NEW;
    END IF;
    IF TG_OP = 'UPDATE' THEN
        NEW.created := OLD.created;
        RETURN NEW;
    END IF;
ELSE 
    IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
        UPDATE abstract_sequence SET modified = now() WHERE id = NEW.id;
        RETURN NEW;
    END IF;
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on abstract_sequence or its children tables.';
END;
$$;

COMMENT ON FUNCTION public.trigger_set_abstract_sequence_modified() IS 'Rewrites created and modified columns of abstract_sequence table with current values.';

CREATE FUNCTION public.trigger_set_element_modified() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF TG_TABLE_NAME = 'element' THEN
    NEW.modified := now();
    IF TG_OP = 'INSERT' THEN
        NEW.created := now();
        RETURN NEW;
    END IF;
    IF TG_OP = 'UPDATE' THEN
        NEW.created := OLD.created;
        RETURN NEW;
    END IF;
ELSE 
    IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
        UPDATE element SET modified = now() WHERE id = NEW.id;
        RETURN NEW;
    END IF;
END IF;
RAISE EXCEPTION 'Unknown operation. This trigger only operates on INSERT and UPDATE operation on element or its children tables.';
END;
$$;

COMMENT ON FUNCTION public.trigger_set_element_modified() IS 'Rewrites created and modified columns of element table with current values.';

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
    "AccessFailedCount" integer NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL
);

COMMENT ON COLUMN public."AspNetUsers".created IS 'User creation date and time (filled trough trigger).';

COMMENT ON COLUMN public."AspNetUsers".modified IS 'User last change date and time (updated trough trigger).';

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

CREATE TABLE public.abstract_sequence (
    id bigint NOT NULL,
    remote_id character varying(255),
    remote_db smallint,
    created timestamp with time zone DEFAULT now() NOT NULL,
    creator_id integer NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    modifier_id integer NOT NULL,
    CONSTRAINT chk_remote_id CHECK ((((remote_db IS NULL) AND (remote_id IS NULL)) OR ((remote_db IS NOT NULL) AND (remote_id IS NOT NULL))))
);

COMMENT ON TABLE public.abstract_sequence IS 'Base table that contains keys for all sequences tables and used for foreign key references. Also contains some common information for all sequences.';

COMMENT ON COLUMN public.abstract_sequence.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.abstract_sequence.remote_id IS 'Id of the sequence in remote database.';

COMMENT ON COLUMN public.abstract_sequence.remote_db IS 'Enum numeric value of the remote db from which sequence is downloaded.';

COMMENT ON COLUMN public.abstract_sequence.created IS 'Sequence creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.abstract_sequence.creator_id IS 'Id of the user that created sequence.';

COMMENT ON COLUMN public.abstract_sequence.modified IS 'Record last change date and time (updated trough trigger).';

COMMENT ON COLUMN public.abstract_sequence.modifier_id IS 'Id of the user that last modified the sequence.';

ALTER TABLE public.abstract_sequence ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.abstract_sequence_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.accordance_characteristic (
    id bigint NOT NULL,
    first_sequence_id bigint NOT NULL,
    second_sequence_id bigint NOT NULL,
    value double precision NOT NULL,
    first_element_id bigint NOT NULL,
    second_element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.accordance_characteristic IS 'Contains numeric chracteristics of accordance of element in different sequences.';

COMMENT ON COLUMN public.accordance_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.accordance_characteristic.first_sequence_id IS 'Id of the first sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.second_sequence_id IS 'Id of the second sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.accordance_characteristic.first_element_id IS 'Id of the element of the first sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.second_element_id IS 'Id of the element of the second sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.accordance_characteristic.characteristic_link_id IS 'Characteristic type id.';

ALTER TABLE public.accordance_characteristic ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.accordance_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.accordance_characteristic_link (
    id smallint NOT NULL,
    accordance_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    CONSTRAINT chk_accordance_characteristic CHECK ((accordance_characteristic = ANY (ARRAY[1, 2]))),
    CONSTRAINT chk_accordance_characteristic_link CHECK ((link = ANY (ARRAY[2, 3, 6, 7])))
);

COMMENT ON TABLE public.accordance_characteristic_link IS 'Contatins list of possible combinations of accordance characteristics parameters.';

COMMENT ON COLUMN public.accordance_characteristic_link.id IS 'Unique identifier.';

COMMENT ON COLUMN public.accordance_characteristic_link.accordance_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.accordance_characteristic_link.link IS 'Link enum numeric value.';

ALTER TABLE public.accordance_characteristic_link ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.accordance_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.binary_characteristic (
    id bigint NOT NULL,
    sequence_id bigint NOT NULL,
    value double precision NOT NULL,
    first_element_id bigint NOT NULL,
    second_element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.binary_characteristic IS 'Contains numeric chracteristics of elements dependece based on their arrangement in sequence.';

COMMENT ON COLUMN public.binary_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.binary_characteristic.sequence_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.binary_characteristic.first_element_id IS 'Id of the first element of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.second_element_id IS 'Id of the second element of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.binary_characteristic.characteristic_link_id IS 'Characteristic type id.';

ALTER TABLE public.binary_characteristic ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.binary_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.binary_characteristic_link (
    id smallint NOT NULL,
    binary_characteristic smallint NOT NULL,
    link smallint NOT NULL,
    CONSTRAINT chk_binary_characteristic CHECK ((binary_characteristic = ANY (ARRAY[1, 2, 3, 4, 5, 6]))),
    CONSTRAINT chk_binary_characteristic_link CHECK ((link = ANY (ARRAY[2, 3, 4])))
);

COMMENT ON TABLE public.binary_characteristic_link IS 'Contatins list of possible combinations of dependence characteristics parameters.';

COMMENT ON COLUMN public.binary_characteristic_link.id IS 'Unique identifier.';

COMMENT ON COLUMN public.binary_characteristic_link.binary_characteristic IS 'Characteristic enum numeric value.';

COMMENT ON COLUMN public.binary_characteristic_link.link IS 'Link enum numeric value.';

ALTER TABLE public.binary_characteristic_link ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.binary_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.sequence_attribute (
    id bigint NOT NULL,
    sequence_id bigint NOT NULL,
    attribute smallint NOT NULL,
    value text NOT NULL,
    CONSTRAINT chk_sequence_attribute_attribute CHECK ((attribute = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48])))
);

COMMENT ON TABLE public.sequence_attribute IS 'Contains sequences'' attributes and their values.';

COMMENT ON COLUMN public.sequence_attribute.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.sequence_attribute.sequence_id IS 'Id of the sequence to which attribute belongs.';

COMMENT ON COLUMN public.sequence_attribute.attribute IS 'Attribute enum numeric value.';

COMMENT ON COLUMN public.sequence_attribute.value IS 'Text of the attribute.';

ALTER TABLE public.sequence_attribute ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.chain_attribute_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.congeneric_characteristic (
    id bigint NOT NULL,
    sequence_id bigint NOT NULL,
    value double precision NOT NULL,
    element_id bigint NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.congeneric_characteristic IS 'Contains numeric chracteristics of congeneric sequences.';

COMMENT ON COLUMN public.congeneric_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.congeneric_characteristic.sequence_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.congeneric_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.congeneric_characteristic.element_id IS 'Id of the element for which the characteristic is calculated.';

COMMENT ON COLUMN public.congeneric_characteristic.characteristic_link_id IS 'Characteristic type id.';

ALTER TABLE public.congeneric_characteristic ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.congeneric_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

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

ALTER TABLE public.congeneric_characteristic_link ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.congeneric_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.element (
    id bigint NOT NULL,
    value character varying(255) NOT NULL,
    description text,
    name character varying(255),
    notation smallint NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT chk_element_notation CHECK ((notation = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13])))
);

COMMENT ON TABLE public.element IS 'Base table for all elements that are stored in the database and used in alphabets of sequences.';

COMMENT ON COLUMN public.element.id IS 'Unique internal identifier of the element.';

COMMENT ON COLUMN public.element.value IS 'Content of the element.';

COMMENT ON COLUMN public.element.description IS 'Description of the element.';

COMMENT ON COLUMN public.element.name IS 'Name of the element.';

COMMENT ON COLUMN public.element.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.element.created IS 'Element creation date and time (filled trough trigger).';

COMMENT ON COLUMN public.element.modified IS 'Record last change date and time (updated trough trigger).';

ALTER TABLE public.element ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.element_new_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE SEQUENCE public.elements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE public.fmotif (
    id bigint NOT NULL,
    alphabet bigint[] NOT NULL,
    "order" integer[] NOT NULL,
    fmotif_type smallint NOT NULL,
    CONSTRAINT chk_fmotif_type CHECK ((fmotif_type = ANY (ARRAY[1, 2, 3, 4, 5])))
);

COMMENT ON TABLE public.fmotif IS 'Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences.';

COMMENT ON COLUMN public.fmotif.id IS 'Unique internal identifier of the fmotif.';

COMMENT ON COLUMN public.fmotif.alphabet IS 'Fmotif''s alphabet (array of notes ids).';

COMMENT ON COLUMN public.fmotif."order" IS 'Fmotif''s order.';

COMMENT ON COLUMN public.fmotif.fmotif_type IS 'Fmotif type enum numeric value.';

CREATE TABLE public.full_characteristic (
    id bigint NOT NULL,
    sequence_id bigint NOT NULL,
    value double precision NOT NULL,
    characteristic_link_id smallint NOT NULL
);

COMMENT ON TABLE public.full_characteristic IS 'Contains numeric chracteristics of complete sequences.';

COMMENT ON COLUMN public.full_characteristic.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.full_characteristic.sequence_id IS 'Id of the sequence for which the characteristic is calculated.';

COMMENT ON COLUMN public.full_characteristic.value IS 'Numerical value of the characteristic.';

COMMENT ON COLUMN public.full_characteristic.characteristic_link_id IS 'Characteristic type id.';

ALTER TABLE public.full_characteristic ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.full_characteristic_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

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

ALTER TABLE public.full_characteristic_link ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.full_characteristic_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.image_sequence (
    id bigint NOT NULL,
    notation smallint NOT NULL,
    order_extractor smallint NOT NULL,
    image_transformations smallint[] NOT NULL,
    matrix_transformations smallint[] NOT NULL,
    research_object_id bigint NOT NULL
);

COMMENT ON TABLE public.image_sequence IS 'Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables.';

COMMENT ON COLUMN public.image_sequence.id IS 'Unique internal identifier of the image sequence.';

COMMENT ON COLUMN public.image_sequence.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.image_sequence.order_extractor IS 'Order extractor enum numeric value used in the process of creation of the sequence.';

COMMENT ON COLUMN public.image_sequence.image_transformations IS 'Array of image transformations applied begore the extraction of the sequence.';

COMMENT ON COLUMN public.image_sequence.matrix_transformations IS 'Array of matrix transformations applied begore the extraction of the sequence.';

COMMENT ON COLUMN public.image_sequence.research_object_id IS 'Id of the research object (image) to which the sequence belongs.';

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
    CONSTRAINT chk_multisequence_reference CHECK ((((multisequence_id IS NULL) AND (multisequence_number IS NULL)) OR ((multisequence_id IS NOT NULL) AND (multisequence_number IS NOT NULL)))),
    CONSTRAINT chk_research_object_nature CHECK ((nature = ANY (ARRAY[1, 2, 3, 4, 5]))),
    CONSTRAINT chk_research_object_sequence_type CHECK ((sequence_type = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14])))
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

ALTER TABLE public.matter ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.matter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.measure (
    id bigint NOT NULL,
    alphabet bigint[] NOT NULL,
    "order" integer[] NOT NULL,
    beats integer NOT NULL,
    beatbase integer NOT NULL,
    fifths integer NOT NULL,
    major boolean NOT NULL
);

COMMENT ON TABLE public.measure IS 'Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences.';

COMMENT ON COLUMN public.measure.id IS 'Unique internal identifier of the measure.';

COMMENT ON COLUMN public.measure.alphabet IS 'Measure alphabet (array of notes ids).';

COMMENT ON COLUMN public.measure."order" IS 'Measure order.';

COMMENT ON COLUMN public.measure.beats IS 'Time signature upper numeral (Beat numerator).';

COMMENT ON COLUMN public.measure.beatbase IS 'Time signature lower numeral (Beat denominator).';

COMMENT ON COLUMN public.measure.fifths IS 'Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).';

COMMENT ON COLUMN public.measure.major IS 'Music mode of the measure. true  represents major and false represents minor.';

CREATE TABLE public.multisequence (
    id integer NOT NULL,
    name text NOT NULL,
    nature smallint DEFAULT 1 NOT NULL,
    CONSTRAINT chk_multisequence_nature CHECK ((nature = ANY (ARRAY[1, 2, 3, 4, 5])))
);

COMMENT ON TABLE public.multisequence IS 'Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups.';

COMMENT ON COLUMN public.multisequence.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.multisequence.name IS 'Multisequence name.';

COMMENT ON COLUMN public.multisequence.nature IS 'Multisequence nature enum numeric value.';

ALTER TABLE public.multisequence ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.multisequence_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.note (
    id bigint NOT NULL,
    numerator integer NOT NULL,
    denominator integer NOT NULL,
    triplet boolean NOT NULL,
    tie smallint DEFAULT 0 NOT NULL,
    CONSTRAINT chk_note_tie CHECK ((tie = ANY (ARRAY[0, 1, 2, 3])))
);

COMMENT ON TABLE public.note IS 'Contains elements that represent notes that are used as elements of music sequences.';

COMMENT ON COLUMN public.note.id IS 'Unique internal identifier.';

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

CREATE TABLE public.pitch (
    id integer NOT NULL,
    octave integer NOT NULL,
    midinumber integer NOT NULL,
    instrument smallint DEFAULT 0 NOT NULL,
    accidental smallint DEFAULT 0 NOT NULL,
    note_symbol smallint NOT NULL,
    CONSTRAINT chk_pitch_accidental CHECK ((accidental = ANY (ARRAY['-2'::integer, '-1'::integer, 0, 1, 2]))),
    CONSTRAINT chk_pitch_note_symbol CHECK ((note_symbol = ANY (ARRAY[0, 2, 4, 5, 7, 9, 11])))
);

COMMENT ON TABLE public.pitch IS 'Note''s pitch.';

COMMENT ON COLUMN public.pitch.id IS 'Unique internal identifier of the pitch.';

COMMENT ON COLUMN public.pitch.octave IS 'Octave number.';

COMMENT ON COLUMN public.pitch.midinumber IS 'Unique number by midi standard.';

COMMENT ON COLUMN public.pitch.instrument IS 'Pitch instrument enum numeric value.';

COMMENT ON COLUMN public.pitch.accidental IS 'Pitch key signature enum numeric value.';

COMMENT ON COLUMN public.pitch.note_symbol IS 'Note symbol enum numeric value.';

ALTER TABLE public.pitch ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.pitch_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

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

ALTER TABLE public."position" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.position_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.sequence (
    id bigint NOT NULL,
    nature smallint NOT NULL,
    notation smallint NOT NULL,
    research_object_id bigint NOT NULL,
    alphabet bigint[] NOT NULL,
    "order" integer[] NOT NULL,
    original boolean DEFAULT true,
    language smallint,
    translator smallint DEFAULT 0,
    pause_treatment smallint DEFAULT 0,
    sequential_transfer boolean DEFAULT false,
    partial boolean DEFAULT false,
    CONSTRAINT chk_original_translator CHECK (((original AND (translator = 0)) OR (NOT original))),
    CONSTRAINT chk_pause_treatment_and_sequential_transfer CHECK (((nature <> 2) OR ((notation = 6) AND (pause_treatment <> 0)) OR (((notation = 7) OR (notation = 8)) AND (pause_treatment = 0) AND (NOT sequential_transfer)))),
    CONSTRAINT chk_sequence_genetic CHECK ((((nature = 1) AND (partial IS NOT NULL)) OR ((nature <> 1) AND (partial IS NULL)))),
    CONSTRAINT chk_sequence_literature CHECK ((((nature = 3) AND (original IS NOT NULL) AND (language IS NOT NULL) AND (translator IS NOT NULL)) OR ((nature <> 3) AND (original IS NULL) AND (language IS NULL) AND (translator IS NULL)))),
    CONSTRAINT chk_sequence_music CHECK ((((nature = 2) AND (pause_treatment IS NOT NULL) AND (sequential_transfer IS NOT NULL)) OR ((nature <> 2) AND (pause_treatment IS NULL) AND (sequential_transfer IS NULL))))
);

COMMENT ON TABLE public.sequence IS 'Table storing all sequences with alphabet and order.';

COMMENT ON COLUMN public.sequence.id IS 'Unique internal identifier of the sequence.';

COMMENT ON COLUMN public.sequence.nature IS 'Sequence''s nature enum numeric value. Used as discriminatior.';

COMMENT ON COLUMN public.sequence.notation IS 'Notation enum numeric value.';

COMMENT ON COLUMN public.sequence.research_object_id IS 'Id of the research object to which the sequence belongs.';

COMMENT ON COLUMN public.sequence.alphabet IS 'Sequence''s alphabet (array of elements ids).';

COMMENT ON COLUMN public.sequence."order" IS 'Sequence''s order.';

COMMENT ON COLUMN public.sequence.original IS 'Flag indicating if this sequence is in original language or was translated.';

COMMENT ON COLUMN public.sequence.language IS 'Primary language of literary work.';

COMMENT ON COLUMN public.sequence.translator IS 'Author of translation or automated translator.';

COMMENT ON COLUMN public.sequence.pause_treatment IS 'Pause treatment enum numeric value.';

COMMENT ON COLUMN public.sequence.sequential_transfer IS 'Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.';

COMMENT ON COLUMN public.sequence.partial IS 'Flag indicating whether sequence is partial or complete.';

CREATE TABLE public.sequence_group (
    id integer NOT NULL,
    name text NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    creator_id integer NOT NULL,
    modified timestamp with time zone DEFAULT now() NOT NULL,
    modifier_id integer NOT NULL,
    nature smallint NOT NULL,
    sequence_group_type smallint,
    sequence_type smallint NOT NULL,
    "group" smallint NOT NULL,
    CONSTRAINT chk_sequence_group_group CHECK (("group" = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]))),
    CONSTRAINT chk_sequence_group_nature CHECK ((nature = ANY (ARRAY[1, 2, 3, 4, 5]))),
    CONSTRAINT chk_sequence_group_sequence_type CHECK ((sequence_type = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]))),
    CONSTRAINT chk_sequence_group_type CHECK ((sequence_group_type = ANY (ARRAY[1, 2, 3, 4, 5])))
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

COMMENT ON COLUMN public.sequence_group.sequence_type IS 'Sequence type enum numeric value.';

COMMENT ON COLUMN public.sequence_group."group" IS 'Group enum numeric value.';

ALTER TABLE public.sequence_group ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sequence_group_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.sequence_group_research_object (
    group_id integer NOT NULL,
    research_object_id bigint NOT NULL
);

COMMENT ON TABLE public.sequence_group_research_object IS 'Intermediate table for infromation on research objects belonging to groups.';

COMMENT ON COLUMN public.sequence_group_research_object.group_id IS 'Sequence group id.';

COMMENT ON COLUMN public.sequence_group_research_object.research_object_id IS 'Research object id.';

CREATE TABLE public.subsequence (
    id bigint NOT NULL,
    sequence_id bigint NOT NULL,
    notation smallint NOT NULL,
    start integer NOT NULL,
    length integer NOT NULL,
    feature smallint NOT NULL,
    partial boolean DEFAULT false NOT NULL,
    CONSTRAINT chk_subsequence_feature CHECK ((feature = ANY (ARRAY[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38]))),
    CONSTRAINT chk_subsequence_notation CHECK ((notation = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13])))
);

COMMENT ON TABLE public.subsequence IS 'Contains information on location and length of the fragments within complete sequences.';

COMMENT ON COLUMN public.subsequence.id IS 'Unique internal identifier.';

COMMENT ON COLUMN public.subsequence.sequence_id IS 'Parent sequence id.';

COMMENT ON COLUMN public.subsequence.notation IS 'Notation of the subsequence (nucleotides, triplets or aminoacids).';

COMMENT ON COLUMN public.subsequence.start IS 'Index of the fragment beginning (from zero).';

COMMENT ON COLUMN public.subsequence.length IS 'Fragment length.';

COMMENT ON COLUMN public.subsequence.feature IS 'Subsequence feature enum numeric value.';

COMMENT ON COLUMN public.subsequence.partial IS 'Flag indicating whether subsequence is partial or complete.';

CREATE TABLE public.task (
    id bigint NOT NULL,
    task_type smallint NOT NULL,
    description text NOT NULL,
    status smallint NOT NULL,
    user_id integer NOT NULL,
    created timestamp with time zone DEFAULT now() NOT NULL,
    started timestamp with time zone,
    completed timestamp with time zone,
    CONSTRAINT chk_task_status CHECK ((status = ANY (ARRAY[1, 2, 3, 4]))),
    CONSTRAINT chk_task_type CHECK ((task_type = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41])))
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

ALTER TABLE public.task ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.task_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

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

ALTER TABLE public.task_result ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.task_result_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

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

INSERT INTO public.element VALUES (1, 'A', NULL, 'Adenin', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (2, 'G', NULL, 'Guanine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (3, 'C', NULL, 'Cytosine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (4, 'T', NULL, 'Thymine', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (5, 'U', NULL, 'Uracil ', 1, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (6, 'TTT', NULL, 'TTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (7, 'GTA', NULL, 'GTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (8, 'CTG', NULL, 'CTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (9, 'TTA', NULL, 'TTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (10, 'GCG', NULL, 'GCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (11, 'ACC', NULL, 'ACC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (12, 'TCG', NULL, 'TCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (13, 'GAG', NULL, 'GAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (14, 'TAT', NULL, 'TAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (15, 'ACG', NULL, 'ACG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (16, 'AGG', NULL, 'AGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (17, 'ATA', NULL, 'ATA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (18, 'AAG', NULL, 'AAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (19, 'GAT', NULL, 'GAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (20, 'GAC', NULL, 'GAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (21, 'AAC', NULL, 'AAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (22, 'GAA', NULL, 'GAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (23, 'CTC', NULL, 'CTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (24, 'GTT', NULL, 'GTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (25, 'CTA', NULL, 'CTA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (26, 'ATT', NULL, 'ATT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (27, 'CAT', NULL, 'CAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (28, 'TGG', NULL, 'TGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (29, 'GTC', NULL, 'GTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (30, 'TGC', NULL, 'TGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (31, 'TAA', NULL, 'TAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (32, 'CCG', NULL, 'CCG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (33, 'GGA', NULL, 'GGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (34, 'ATC', NULL, 'ATC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (35, 'CGC', NULL, 'CGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (36, 'AGT', NULL, 'AGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (37, 'CAA', NULL, 'CAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (38, 'GCT', NULL, 'GCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (39, 'CGG', NULL, 'CGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (40, 'AAA', NULL, 'AAA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (41, 'CTT', NULL, 'CTT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (42, 'CGA', NULL, 'CGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (43, 'TAG', NULL, 'TAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (44, 'TTG', NULL, 'TTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (45, 'TCC', NULL, 'TCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (46, 'TTC', NULL, 'TTC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (47, 'CCT', NULL, 'CCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (48, 'TGA', NULL, 'TGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (49, 'TCA', NULL, 'TCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (50, 'CAG', NULL, 'CAG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (51, 'CAC', NULL, 'CAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (52, 'GCC', NULL, 'GCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (53, 'GGC', NULL, 'GGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (54, 'GCA', NULL, 'GCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (55, 'TGT', NULL, 'TGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (56, 'TCT', NULL, 'TCT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (57, 'GGG', NULL, 'GGG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (58, 'TAC', NULL, 'TAC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (59, 'CCA', NULL, 'CCA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (60, 'GTG', NULL, 'GTG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (61, 'ACA', NULL, 'ACA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (62, 'CCC', NULL, 'CCC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (63, 'AGA', NULL, 'AGA', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (64, 'ACT', NULL, 'ACT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (65, 'AAT', NULL, 'AAT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (66, 'ATG', NULL, 'ATG', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (67, 'GGT', NULL, 'GGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (68, 'AGC', NULL, 'AGC', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (69, 'CGT', NULL, 'CGT', 2, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (70, 'Y', NULL, 'Tyrosine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (71, 'G', NULL, 'Glycine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (72, 'A', NULL, 'Alanine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (73, 'V', NULL, 'Valine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (74, 'I', NULL, 'Isoleucine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (75, 'L', NULL, 'Leucine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (76, 'P', NULL, 'Proline', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (77, 'S', NULL, 'Serine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (78, 'T', NULL, 'Threonine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (79, 'C', NULL, 'Cysteine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (80, 'M', NULL, 'Methionine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (81, 'D', NULL, 'Aspartic acid', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (82, 'N', NULL, 'Asparagine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (83, 'E', NULL, 'Glutamic acid', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (84, 'Q', NULL, 'Glutamine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (85, 'K', NULL, 'Lysine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (86, 'R', NULL, 'Arginine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (87, 'H', NULL, 'Histidine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (88, 'F', NULL, 'Phenylalanine', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (89, 'W', NULL, 'Tryptophan', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');
INSERT INTO public.element VALUES (90, 'X', NULL, 'Stop codon', 3, '2024-01-22 19:04:49.565079+06', '2024-01-22 19:04:49.565079+06');

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

SELECT pg_catalog.setval('public.abstract_sequence_id_seq', 1, false);

SELECT pg_catalog.setval('public.accordance_characteristic_id_seq', 1, false);

SELECT pg_catalog.setval('public.accordance_characteristic_link_id_seq', 9, false);

SELECT pg_catalog.setval('public.binary_characteristic_id_seq', 1, false);

SELECT pg_catalog.setval('public.binary_characteristic_link_id_seq', 19, false);

SELECT pg_catalog.setval('public.chain_attribute_id_seq', 1, false);

SELECT pg_catalog.setval('public.congeneric_characteristic_id_seq', 1, false);

SELECT pg_catalog.setval('public.congeneric_characteristic_link_id_seq', 97, false);

SELECT pg_catalog.setval('public.element_new_id_seq', 90, true);

SELECT pg_catalog.setval('public.elements_id_seq', 54522478, true);

SELECT pg_catalog.setval('public.full_characteristic_id_seq', 1, false);

SELECT pg_catalog.setval('public.full_characteristic_link_id_seq', 211, false);

SELECT pg_catalog.setval('public.matter_id_seq', 1, false);

SELECT pg_catalog.setval('public.multisequence_id_seq', 1, false);

SELECT pg_catalog.setval('public.pitch_id_seq', 1, false);

SELECT pg_catalog.setval('public.position_id_seq', 1, false);

SELECT pg_catalog.setval('public.sequence_group_id_seq', 1, false);

SELECT pg_catalog.setval('public.task_id_seq', 1, false);

SELECT pg_catalog.setval('public.task_result_id_seq', 1, false);

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

ALTER TABLE ONLY public.abstract_sequence
    ADD CONSTRAINT pk_abstract_sequence PRIMARY KEY (id);

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT pk_accordance_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.accordance_characteristic_link
    ADD CONSTRAINT pk_accordance_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT pk_binary_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.binary_characteristic_link
    ADD CONSTRAINT pk_binary_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT pk_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT pk_congeneric_characteristic PRIMARY KEY (id);

ALTER TABLE ONLY public.congeneric_characteristic_link
    ADD CONSTRAINT pk_congeneric_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.element
    ADD CONSTRAINT pk_element PRIMARY KEY (id);

ALTER TABLE ONLY public.fmotif
    ADD CONSTRAINT pk_fmotif PRIMARY KEY (id);

ALTER TABLE ONLY public.full_characteristic_link
    ADD CONSTRAINT pk_full_characteristic_link PRIMARY KEY (id);

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT pk_image_sequence PRIMARY KEY (id);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT pk_matter PRIMARY KEY (id);

ALTER TABLE ONLY public.measure
    ADD CONSTRAINT pk_measure PRIMARY KEY (id);

ALTER TABLE ONLY public.multisequence
    ADD CONSTRAINT pk_multisequence PRIMARY KEY (id);

ALTER TABLE ONLY public.note
    ADD CONSTRAINT pk_note PRIMARY KEY (id);

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT pk_note_pitch PRIMARY KEY (note_id, pitch_id);

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT pk_piece PRIMARY KEY (id);

ALTER TABLE ONLY public.pitch
    ADD CONSTRAINT pk_pitch PRIMARY KEY (id);

ALTER TABLE ONLY public.sequence
    ADD CONSTRAINT pk_sequence PRIMARY KEY (id);

ALTER TABLE ONLY public.sequence_attribute
    ADD CONSTRAINT pk_sequence_attribute PRIMARY KEY (id);

ALTER TABLE ONLY public.sequence_group_research_object
    ADD CONSTRAINT pk_sequence_group_research_object PRIMARY KEY (research_object_id, group_id);

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT pk_subsequence PRIMARY KEY (id);

ALTER TABLE ONLY public.task
    ADD CONSTRAINT pk_task PRIMARY KEY (id);

ALTER TABLE ONLY public.sequence_group
    ADD CONSTRAINT sequence_group_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.task_result
    ADD CONSTRAINT task_result_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public."AspNetPushNotificationSubscribers"
    ADD CONSTRAINT "uk_AspNetPushNotificationSubscribers" UNIQUE ("UserId", "Endpoint");

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT uk_accordance_characteristic UNIQUE (first_sequence_id, second_sequence_id, first_element_id, second_element_id, characteristic_link_id);

ALTER TABLE ONLY public.accordance_characteristic_link
    ADD CONSTRAINT uk_accordance_characteristic_link UNIQUE (accordance_characteristic, link);

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT uk_binary_characteristic UNIQUE (sequence_id, first_element_id, second_element_id, characteristic_link_id);

ALTER TABLE ONLY public.binary_characteristic_link
    ADD CONSTRAINT uk_binary_characteristic_link UNIQUE (binary_characteristic, link);

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT uk_characteristic UNIQUE (sequence_id, characteristic_link_id);

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT uk_congeneric_characteristic UNIQUE (sequence_id, element_id, characteristic_link_id);

ALTER TABLE ONLY public.congeneric_characteristic_link
    ADD CONSTRAINT uk_congeneric_characteristic_link UNIQUE (congeneric_characteristic, link, arrangement_type);

ALTER TABLE ONLY public.element
    ADD CONSTRAINT uk_element_value_notation UNIQUE (value, notation);

ALTER TABLE ONLY public.full_characteristic_link
    ADD CONSTRAINT uk_full_characteristic_link UNIQUE (full_characteristic, link, arrangement_type);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT uk_matter UNIQUE (name, nature);

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT uk_matter_multisequence UNIQUE (multisequence_id, multisequence_number) DEFERRABLE INITIALLY DEFERRED;

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT uk_matter_nature UNIQUE (id, nature);

ALTER TABLE ONLY public.multisequence
    ADD CONSTRAINT uk_multisequence_name UNIQUE (name);

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT uk_piece UNIQUE (subsequence_id, start, length);

ALTER TABLE ONLY public.pitch
    ADD CONSTRAINT uk_pitch UNIQUE (octave, instrument, accidental, note_symbol);

ALTER TABLE ONLY public.sequence
    ADD CONSTRAINT uk_sequence UNIQUE NULLS NOT DISTINCT (research_object_id, notation, language, translator, pause_treatment, sequential_transfer);

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

CREATE INDEX fki_congeneric_characteristic_alphabet_element ON public.congeneric_characteristic USING btree (sequence_id, element_id);

CREATE INDEX ix_accordance_characteristic_first_sequence_id ON public.accordance_characteristic USING btree (first_sequence_id);

COMMENT ON INDEX public.ix_accordance_characteristic_first_sequence_id IS 'First sequence id index of accordance characteristic.';

CREATE INDEX ix_accordance_characteristic_first_sequence_id_brin ON public.accordance_characteristic USING brin (first_sequence_id);

CREATE INDEX ix_accordance_characteristic_second_sequence_id ON public.accordance_characteristic USING btree (second_sequence_id);

COMMENT ON INDEX public.ix_accordance_characteristic_second_sequence_id IS 'Second sequence id index of accordance characteristic.';

CREATE INDEX ix_accordance_characteristic_second_sequence_id_brin ON public.accordance_characteristic USING brin (second_sequence_id);

CREATE INDEX ix_accordance_characteristic_sequences_ids_brin ON public.accordance_characteristic USING brin (first_sequence_id, second_sequence_id);

CREATE INDEX ix_accordance_characteristic_sequences_ids_characteristic_link_ ON public.accordance_characteristic USING brin (first_sequence_id, second_sequence_id, characteristic_link_id);

CREATE INDEX ix_binary_characteristic_first_sequence_id_brin ON public.binary_characteristic USING brin (sequence_id);

CREATE INDEX ix_binary_characteristic_sequence_id ON public.binary_characteristic USING btree (sequence_id);

COMMENT ON INDEX public.ix_binary_characteristic_sequence_id IS 'Sequence id index of binary characteristic.';

CREATE INDEX ix_binary_characteristic_sequence_id_characteristic_link_id_bri ON public.binary_characteristic USING brin (sequence_id, characteristic_link_id);

CREATE INDEX ix_characteristic_characteristic_type_link ON public.full_characteristic USING btree (characteristic_link_id);

CREATE INDEX ix_characteristic_sequence_id ON public.full_characteristic USING btree (sequence_id);

COMMENT ON INDEX public.ix_characteristic_sequence_id IS 'Sequence id index of integral characteristic.';

CREATE INDEX ix_congeneric_characteristic_characteristic_link_id_brin ON public.congeneric_characteristic USING brin (characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id ON public.congeneric_characteristic USING btree (sequence_id);

COMMENT ON INDEX public.ix_congeneric_characteristic_sequence_id IS 'Sequence id index of congeneric characteristic.';

CREATE INDEX ix_congeneric_characteristic_sequence_id_brin ON public.congeneric_characteristic USING brin (sequence_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_characteristic_link_id ON public.congeneric_characteristic USING brin (sequence_id, characteristic_link_id);

CREATE INDEX ix_congeneric_characteristic_sequence_id_element_id_brin ON public.congeneric_characteristic USING brin (sequence_id, element_id);

CREATE INDEX ix_element_notation ON public.element USING btree (notation);

COMMENT ON INDEX public.ix_element_notation IS 'Index on notation of elements.';

CREATE INDEX ix_element_value ON public.element USING btree (value);

COMMENT ON INDEX public.ix_element_value IS 'Index on value of elements.';

CREATE INDEX ix_fmotif_alphabet ON public.fmotif USING gin (alphabet);

CREATE INDEX ix_full_characteristic_characteristic_link_id_brin ON public.full_characteristic USING brin (characteristic_link_id);

CREATE INDEX ix_full_characteristic_sequence_id_brin ON public.full_characteristic USING brin (sequence_id);

CREATE INDEX ix_image_sequence_research_object_id ON public.image_sequence USING btree (research_object_id);

CREATE INDEX ix_matter_nature ON public.matter USING btree (nature);

COMMENT ON INDEX public.ix_matter_nature IS 'Индекс по природе таблицы matter.';

CREATE INDEX ix_measure_alphabet ON public.measure USING gin (alphabet);

CREATE INDEX ix_position_subsequence_id ON public."position" USING btree (subsequence_id);

CREATE INDEX ix_position_subsequence_id_brin ON public."position" USING brin (subsequence_id);

CREATE INDEX ix_sequence_alphabet ON public.sequence USING gin (alphabet);

CREATE INDEX ix_sequence_attribute_sequence_id_brin ON public.sequence_attribute USING brin (sequence_id);

CREATE INDEX ix_sequence_notation ON public.sequence USING btree (notation);

COMMENT ON INDEX public.ix_sequence_notation IS 'Index on sequence''s notation.';

CREATE INDEX ix_sequence_research_object_id ON public.sequence USING btree (research_object_id);

COMMENT ON INDEX public.ix_sequence_research_object_id IS 'Index on reserch objects sequences blog to.';

CREATE INDEX ix_sequence_research_object_id_language ON public.sequence USING btree (research_object_id, language) WITH (deduplicate_items='true');

CREATE INDEX ix_sequence_research_object_id_nature ON public.sequence USING btree (research_object_id, nature) WITH (deduplicate_items='true');

CREATE INDEX ix_subsequence_feature_brin ON public.subsequence USING brin (feature);

CREATE INDEX ix_subsequence_sequence_id ON public.subsequence USING btree (sequence_id);

CREATE INDEX ix_subsequence_sequence_id_brin ON public.subsequence USING brin (sequence_id);

CREATE INDEX ix_subsequence_sequence_id_notation_feature_brin ON public.subsequence USING brin (sequence_id, notation, feature);

CREATE INDEX ix_subsequence_sequence_notation_feature ON public.subsequence USING btree (sequence_id, notation, feature);

CREATE INDEX ix_task_user_id ON public.task USING btree (user_id);

CREATE UNIQUE INDEX uk_sequence_attribute ON public.sequence_attribute USING btree (sequence_id, attribute, md5(value));

CREATE TRIGGER "tgiu_AspNetUsers_modified" BEFORE INSERT OR UPDATE ON public."AspNetUsers" FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER "tgiu_AspNetUsers_modified" ON public."AspNetUsers" IS 'Trigger adding creation and modification dates.';

CREATE TRIGGER tgiu_abstract_sequence_modified BEFORE INSERT OR UPDATE ON public.abstract_sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_abstract_sequence_modified();

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabet BEFORE INSERT OR UPDATE OF first_element_id, second_element_id, sequence_id ON public.binary_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_elements_in_alphabet();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabet ON public.binary_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_binary_chracteristic_elements_in_alphabets BEFORE INSERT OR UPDATE OF first_sequence_id, second_sequence_id, first_element_id, second_element_id ON public.accordance_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_elements_in_alphabets();

COMMENT ON TRIGGER tgiu_binary_chracteristic_elements_in_alphabets ON public.accordance_characteristic IS 'Триггер, проверяющий что оба элемента связываемые коэффициентом зависимости присутствуют в алфавите данной цепочки.';

CREATE TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet BEFORE INSERT OR UPDATE OF element_id, sequence_id ON public.congeneric_characteristic FOR EACH ROW EXECUTE FUNCTION public.trigger_check_element_in_alphabet();

COMMENT ON TRIGGER tgiu_congeneric_chracteristic_element_in_alphabet ON public.congeneric_characteristic IS 'Триггер, проверяющий что элемент однородной цепочки, для которой вычислена характеристика, присутствует в алфавите данной цепочки.';

CREATE TRIGGER tgiu_element_modified BEFORE INSERT OR UPDATE ON public.element FOR EACH ROW EXECUTE FUNCTION public.trigger_set_element_modified();

COMMENT ON TRIGGER tgiu_element_modified ON public.element IS 'Trigger for rewriting created and modified fields with actual dates.';

CREATE TRIGGER tgiu_fmotif_check_alphabet BEFORE INSERT OR UPDATE OF alphabet ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_check_notes_alphabet();

COMMENT ON TRIGGER tgiu_fmotif_check_alphabet ON public.fmotif IS 'Checks that all alphabet elements (notes) are present in database.';

CREATE TRIGGER tgiu_fmotif_check_order BEFORE INSERT OR UPDATE OF alphabet, "order" ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_check_order();

COMMENT ON TRIGGER tgiu_fmotif_check_order ON public.fmotif IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_fmotif_modified BEFORE INSERT OR UPDATE ON public.fmotif FOR EACH ROW EXECUTE FUNCTION public.trigger_set_element_modified();

COMMENT ON TRIGGER tgiu_fmotif_modified ON public.fmotif IS 'Trigger for rewriting created and modified fields with actual dates.';

CREATE TRIGGER tgiu_image_sequence_modified BEFORE INSERT OR UPDATE ON public.image_sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_abstract_sequence_modified();

CREATE TRIGGER tgiu_matter_modified BEFORE INSERT OR UPDATE ON public.matter FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_matter_modified ON public.matter IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_measure_check_alphabet BEFORE INSERT OR UPDATE OF alphabet ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_check_notes_alphabet();

COMMENT ON TRIGGER tgiu_measure_check_alphabet ON public.measure IS 'Checks that all alphabet elements (notes) are present in database.';

CREATE TRIGGER tgiu_measure_check_order BEFORE INSERT OR UPDATE OF alphabet, "order" ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_check_order();

COMMENT ON TRIGGER tgiu_measure_check_order ON public.measure IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_measure_modified BEFORE INSERT OR UPDATE ON public.measure FOR EACH ROW EXECUTE FUNCTION public.trigger_set_element_modified();

COMMENT ON TRIGGER tgiu_measure_modified ON public.measure IS 'Trigger for rewriting created and modified fields with actual dates.';

CREATE TRIGGER tgiu_note_modified BEFORE INSERT OR UPDATE ON public.note FOR EACH ROW EXECUTE FUNCTION public.trigger_set_element_modified();

COMMENT ON TRIGGER tgiu_note_modified ON public.note IS 'Trigger for rewriting created and modified fields with actual dates.';

CREATE TRIGGER tgiu_sequence_alphabet_check BEFORE INSERT OR UPDATE OF alphabet ON public.sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_check_alphabet();

COMMENT ON TRIGGER tgiu_sequence_alphabet_check ON public.sequence IS 'Checks that all alphabet elements are present in database.';

CREATE TRIGGER tgiu_sequence_group_modified BEFORE INSERT OR UPDATE ON public.sequence_group FOR EACH ROW EXECUTE FUNCTION public.trigger_set_modified();

COMMENT ON TRIGGER tgiu_sequence_group_modified ON public.sequence_group IS 'Тригер для вставки даты последнего изменения записи.';

CREATE TRIGGER tgiu_sequence_modified BEFORE INSERT OR UPDATE ON public.sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_abstract_sequence_modified();

CREATE TRIGGER tgiu_sequence_order_check BEFORE INSERT OR UPDATE OF "order" ON public.sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_check_order();

COMMENT ON TRIGGER tgiu_sequence_order_check ON public.sequence IS 'Validates order of the sequence and checks its consistency with the alphabet.';

CREATE TRIGGER tgiu_subsequence_modified BEFORE INSERT OR UPDATE ON public.subsequence FOR EACH ROW EXECUTE FUNCTION public.trigger_set_abstract_sequence_modified();

COMMENT ON TRIGGER tgiu_subsequence_modified ON public.subsequence IS 'Trigger adding creation and modification dates in abstract_sequence table.';

CREATE TRIGGER tgu_delete_image_sequence_characteristics AFTER UPDATE OF order_extractor, image_transformations, matrix_transformations ON public.image_sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_sequence_characteristics();

COMMENT ON TRIGGER tgu_delete_image_sequence_characteristics ON public.image_sequence IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_delete_sequence_characteristics AFTER UPDATE OF alphabet, "order" ON public.sequence FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_sequence_characteristics();

COMMENT ON TRIGGER tgu_delete_sequence_characteristics ON public.sequence IS 'Trigger deleting all characteristics of sequences that has been updated.';

CREATE TRIGGER tgu_delete_subsequence_characteristics AFTER UPDATE OF start, length, sequence_id ON public.subsequence FOR EACH ROW EXECUTE FUNCTION public.trigger_delete_sequence_characteristics();

COMMENT ON TRIGGER tgu_delete_subsequence_characteristics ON public.subsequence IS 'Trigger deleting all characteristics of subsequences that has been updated.';

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
    ADD CONSTRAINT fk_accordance_characteristic_element_first FOREIGN KEY (first_element_id) REFERENCES public.element(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_element_second FOREIGN KEY (second_element_id) REFERENCES public.element(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_first_abstract_sequence FOREIGN KEY (first_sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.accordance_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.accordance_characteristic
    ADD CONSTRAINT fk_accordance_characteristic_second_abstract_sequence FOREIGN KEY (second_sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_abstract_sequence FOREIGN KEY (sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_first FOREIGN KEY (first_element_id) REFERENCES public.element(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_element_second FOREIGN KEY (second_element_id) REFERENCES public.element(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.binary_characteristic
    ADD CONSTRAINT fk_binary_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.binary_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT fk_characteristic_abstract_sequence FOREIGN KEY (sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_abstract_sequence FOREIGN KEY (sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_element FOREIGN KEY (element_id) REFERENCES public.element(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.congeneric_characteristic
    ADD CONSTRAINT fk_congeneric_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.congeneric_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.fmotif
    ADD CONSTRAINT fk_fmotif_element FOREIGN KEY (id) REFERENCES public.element(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.full_characteristic
    ADD CONSTRAINT fk_full_characteristic_link FOREIGN KEY (characteristic_link_id) REFERENCES public.full_characteristic_link(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT fk_image_sequence_abstract_sequence FOREIGN KEY (id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.image_sequence
    ADD CONSTRAINT fk_image_sequence_research_object FOREIGN KEY (research_object_id) REFERENCES public.matter(id) ON UPDATE CASCADE;

ALTER TABLE ONLY public.matter
    ADD CONSTRAINT fk_matter_multisequence FOREIGN KEY (multisequence_id) REFERENCES public.multisequence(id) ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE ONLY public.sequence_group_research_object
    ADD CONSTRAINT fk_matter_sequence_group FOREIGN KEY (group_id) REFERENCES public.sequence_group(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.measure
    ADD CONSTRAINT fk_measure_element FOREIGN KEY (id) REFERENCES public.element(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.note
    ADD CONSTRAINT fk_note_element FOREIGN KEY (id) REFERENCES public.element(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT fk_note_pitch_note FOREIGN KEY (note_id) REFERENCES public.note(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.note_pitch
    ADD CONSTRAINT fk_note_pitch_pitch FOREIGN KEY (pitch_id) REFERENCES public.pitch(id) ON UPDATE RESTRICT ON DELETE RESTRICT;

ALTER TABLE ONLY public."position"
    ADD CONSTRAINT fk_position_subsequence FOREIGN KEY (subsequence_id) REFERENCES public.subsequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.sequence
    ADD CONSTRAINT fk_sequence_abstract_sequence FOREIGN KEY (id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.sequence_attribute
    ADD CONSTRAINT fk_sequence_attribute_abstract_sequence FOREIGN KEY (sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.sequence_group_research_object
    ADD CONSTRAINT fk_sequence_group_research_object FOREIGN KEY (research_object_id) REFERENCES public.matter(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.sequence
    ADD CONSTRAINT fk_sequence_research_object FOREIGN KEY (research_object_id, nature) REFERENCES public.matter(id, nature) ON UPDATE CASCADE;

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT fk_subsequence_abstract_sequence FOREIGN KEY (id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.subsequence
    ADD CONSTRAINT fk_subsequence_sequence_abstract_sequence FOREIGN KEY (sequence_id) REFERENCES public.abstract_sequence(id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.task
    ADD CONSTRAINT "fk_task_AspNetUsers" FOREIGN KEY (user_id) REFERENCES public."AspNetUsers"("Id") ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.task_result
    ADD CONSTRAINT fk_task_result_task FOREIGN KEY (task_id) REFERENCES public.task(id) ON UPDATE CASCADE ON DELETE CASCADE;

COMMIT;
--20.01.2025 14:33:09
