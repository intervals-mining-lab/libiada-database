﻿namespace Libiada.Database.Models.Repositories.Sequences;

using Libiada.Database.Extensions;

using Npgsql;
using NpgsqlTypes;

using System;
using System.Collections.Generic;

public class ImageSequenceRepository
{
    public void Create(ImageSequence sequence, LibiadaDatabaseEntities db)
    {
        if (sequence.Id == default)
        {
            sequence.Id = db.GetNewElementId();
        }

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long>("id", NpgsqlDbType.Bigint) { TypedValue = sequence.Id },
            new NpgsqlParameter<byte>("notation", NpgsqlDbType.Smallint){ TypedValue = (byte)sequence.Notation },
            new NpgsqlParameter<long>("matter_id", NpgsqlDbType.Bigint){ TypedValue = sequence.MatterId },
            new NpgsqlParameter<byte[]>("image_transformations", NpgsqlDbType.Array | NpgsqlDbType.Smallint){ TypedValue = Array.Empty<byte>() },
            new NpgsqlParameter<byte[]>("matrix_transformations", NpgsqlDbType.Array | NpgsqlDbType.Smallint){ TypedValue = Array.Empty<byte>() },
            new NpgsqlParameter<byte>("order_extractor", NpgsqlDbType.Smallint){ TypedValue = (byte)sequence.OrderExtractor },

        };

        const string Query = @"INSERT INTO image_sequence (
                                        id,
                                        notation,
                                        matter_id,
                                        image_transformations,
                                        matrix_transformations,
                                        order_extractor
                                    ) VALUES (
                                        @id,
                                        @notation,
                                        @matter_id,
                                        @image_transformations,
                                        @matrix_transformations,
                                        @order_extractor
                                    );";

        db.ExecuteCommand(Query, parameters.ToArray());
    }
}
