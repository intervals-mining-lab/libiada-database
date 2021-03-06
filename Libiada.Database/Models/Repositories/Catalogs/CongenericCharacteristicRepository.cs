﻿namespace Libiada.Database.Models.Repositories.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using Libiada.Database;
    using LibiadaCore.Core;
    using LibiadaCore.Core.ArrangementManagers;
    using LibiadaCore.Core.Characteristics.Calculators.CongenericCalculators;
    using LibiadaCore.Extensions;

    using Libiada.Database.Helpers;
    using Libiada.Database.Models.CalculatorsData;

    /// <summary>
    /// The congeneric characteristic repository.
    /// </summary>
    public class CongenericCharacteristicRepository
    {
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The instance.
        /// </summary>
        private static volatile CongenericCharacteristicRepository instance;

        /// <summary>
        /// The congeneric characteristic links.
        /// </summary>
        private readonly CongenericCharacteristicLink[] characteristicsLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CongenericCharacteristicRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        private CongenericCharacteristicRepository(LibiadaDatabaseEntities db)
        {
            characteristicsLinks = db.CongenericCharacteristicLink.ToArray();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static CongenericCharacteristicRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            using (var db = new LibiadaDatabaseEntities())
                            {
                                instance = new CongenericCharacteristicRepository(db);
                            }
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the congeneric characteristic links.
        /// </summary>
        public IEnumerable<CongenericCharacteristicLink> CharacteristicLinks => characteristicsLinks.ToArray();

        /// <summary>
        /// The get link for congeneric characteristic.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link GetLinkForCharacteristic(int characteristicLinkId)
        {
            return characteristicsLinks.Single(c => c.Id == characteristicLinkId).Link;
        }

        /// <summary>
        /// The get congeneric characteristic type.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="CongenericCharacteristic"/>.
        /// </returns>
        public CongenericCharacteristic GetCharacteristic(int characteristicLinkId)
        {
            return characteristicsLinks.Single(c => c.Id == characteristicLinkId).CongenericCharacteristic;
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <param name="notation">
        /// The notation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCharacteristicName(int characteristicLinkId, Notation notation)
        {
            return string.Join("  ", GetCharacteristicName(characteristicLinkId), notation.GetDisplayValue());
        }

        /// <summary>
        /// The get congeneric characteristic name.
        /// </summary>
        /// <param name="characteristicLinkId">
        /// The characteristic type link id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCharacteristicName(int characteristicLinkId)
        {
            string characteristicTypeName = GetCharacteristic(characteristicLinkId).GetDisplayValue();

            Link link = GetLinkForCharacteristic(characteristicLinkId);
            string linkName = link == Link.NotApplied ? string.Empty : link.GetDisplayValue();

            return string.Join("  ", characteristicTypeName, linkName);
        }
    }
}