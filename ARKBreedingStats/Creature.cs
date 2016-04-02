﻿using System;
using System.Linq;
using System.Xml.Serialization;

namespace ARKBreedingStats
{
    [Serializable()]
    public class Creature
    {
        public string species;
        public string name;
        public Gender gender;
        public CreatureStatus status;
        // order of the stats is Health, Stamina, Oxygen, Food, Weight, MeleeDamage, Speed, Torpor
        public int[] levelsWild;
        public int[] levelsDom;
        public double tamingEff;
        [XmlIgnore]
        public double[] valuesBreeding = new double[8];
        [XmlIgnore]
        public double[] valuesDom = new double[8];
        [XmlIgnore]
        public bool[] topBreedingStats = new bool[8]; // indexes of stats that are top for that species in the creaturecollection
        [XmlIgnore]
        public Int16 topStatsCount;
        [XmlIgnore]
        public bool topBreedingCreature; // true if it has some topBreedingStats and if it's male, no other male has more topBreedingStats
        [XmlIgnore]
        public Int16 topness; // permille of mean of wildlevels compared to toplevels
        public string owner;
        public string note; // user defined note about that creature
        public Guid guid;
        public bool isBred;
        public Guid fatherGuid;
        public Guid motherGuid;
        [XmlIgnore]
        private Creature father;
        [XmlIgnore]
        private Creature mother;
        [XmlIgnore]
        public int levelFound;
        public int generation; // number of generations from the oldest wild creature
        public int[] colors = new int[6] { 0, 0, 0, 0, 0, 0 }; // id of colors

        public Creature()
        {
        }

        public Creature(string species, string name, string owner, Gender gender, int[] levelsWild, int[] levelsDom = null, double tamingEff = 0, bool isBred = false)
        {
            this.species = species;
            this.name = name;
            this.owner = owner;
            this.gender = (Gender)gender;
            this.levelsWild = levelsWild;
            this.levelsDom = (levelsDom == null ? new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } : levelsDom);
            if (isBred)
                this.tamingEff = 1;
            else
                this.tamingEff = tamingEff;
            this.isBred = isBred;
            this.status = CreatureStatus.Available;
            calculateLevelFound();
        }

        public void calculateLevelFound()
        {
            levelFound = 0;
            if (!isBred && tamingEff >= 0)
                levelFound = (int)Math.Ceiling(levelHatched / (1 + tamingEff / 2)); // TODO due to rounding of ingame TE, it can differ. Round to next multiple of 4?
        }

        [XmlIgnore]
        public int levelHatched { get { return levelsWild[7] + 1; } }
        [XmlIgnore]
        public int level { get { return levelHatched + levelsDom.Sum(); } }

        public void recalculateAncestorGenerations()
        {
            generation = ancestorGenerations();
        }

        /// <summary>
        /// Returns the number of generations to the oldest known ancestor
        /// </summary>
        /// <param name="c">Creature to check</param>
        /// <param name="g">Generations so far</param>
        /// <returns></returns>
        private int ancestorGenerations(int g = 0)
        {
            // to detect loop (if a creature is falsely listed as its own ancestor)
            if (g > 99)
                return 100;

            int mgen = 0, fgen = 0;
            if (mother != null)
                mgen = mother.ancestorGenerations(g + 1);
            if (father != null)
                fgen = father.ancestorGenerations(g + 1);
            if (mgen > fgen)
                return mgen + g;
            else
                return fgen + g;
        }

        [XmlIgnore]
        public Creature Mother
        {
            set
            {
                mother = value;
                motherGuid = (mother != null ? mother.guid : Guid.Empty);
            }
            get { return mother; }
        }
        [XmlIgnore]
        public Creature Father
        {
            set
            {
                father = value;
                fatherGuid = (father != null ? father.guid : Guid.Empty);
            }
            get { return father; }
        }

        public void setTopStatCount(bool[] considerStatHighlight)
        {
            Int16 c = 0;
            for (int s = 0; s < 8; s++)
            {
                if (considerStatHighlight[s] && topBreedingStats[s])
                    c++;
            }
            topStatsCount = c;
        }
    }

    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    };

    public enum CreatureStatus
    {
        Available,
        Dead,
        Unavailable,
        Alive = Available // backwards-compatibility
    };
}