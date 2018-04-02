﻿using ARKBreedingStats.species;
using System;
using System.IO;

namespace ARKBreedingStats
{
    static class ImportExported
    {
        static public CreatureValues importExportedCreature(string filePath)
        {
            CreatureValues cv = new CreatureValues();
            cv.domesticatedAt = File.GetLastWriteTime(filePath);
            cv.isTamed = true;
            cv.tamingEffMax = 100;
            string[] iniLines = File.ReadAllLines(filePath);
            string id = "";
            int statIndexIngame = -1;
            string[] statIndices = new string[] { "Health", "Stamina", "Torpidity", "Oxygen", "Food", "", "", "Weight", "Melee Damage", "Movement Speed", "", "" };
            bool inStatSection = false;
            foreach (string line in iniLines)
            {
                if (line.Contains("="))
                {
                    string parameterName;
                    int i = line.IndexOf("=");
                    string text = line.Substring(i + 1);
                    double.TryParse(text, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out double value);
                    if (inStatSection)
                    {
                        statIndexIngame++;
                        if (statIndexIngame > 11)
                            inStatSection = false;
                    }
                    if (inStatSection)
                        parameterName = statIndices[statIndexIngame];
                    else
                        parameterName = line.Substring(0, i);

                    if (parameterName.Length > 0)
                    {
                        switch (parameterName)
                        {
                            // todo ID1, ID2
                            case "DinoID1":
                                if (string.IsNullOrEmpty(id))
                                {
                                    id = text;
                                }
                                else
                                {
                                    cv.guid = builtGuid(text, id);
                                }
                                break;
                            case "DinoID2":
                                if (string.IsNullOrEmpty(id))
                                {
                                    id = text;
                                }
                                else
                                {
                                    cv.guid = builtGuid(id, text);
                                }
                                break;
                            case "DinoNameTag":
                                cv.species = text;
                                break;
                            case "bIsFemale":
                                cv.sex = (text == "True" ? Sex.Female : Sex.Male);
                                break;
                            case "bIsNeutered":
                                cv.neutered = (text == "False" ? false : true);
                                break;
                            case "TamerString":
                                cv.owner = text;
                                break;
                            case "TamedName":
                                cv.name = text;
                                break;
                            case "ImprinterName":
                                cv.imprinterName = text;
                                if (string.IsNullOrEmpty(cv.owner))
                                    cv.owner = text;
                                break;
                            // todo mutations for mother and father
                            case "RandomMutationsMale":
                                break;
                            case "RandomMutationsFemale":
                                break;
                            case "BabyAge":
                                int speciesIndex = Values.V.speciesIndex(cv.species);
                                if (speciesIndex >= 0 && value >= 0 && value <= 1)
                                    cv.growingUntil = DateTime.Now.AddSeconds((int)(Values.V.species[speciesIndex].breeding.maturationTimeAdjusted * (1 - value)));
                                break;
                            case "CharacterLevel":
                                cv.level = (int)value;
                                break;
                            case "DinoImprintingQuality":
                                cv.imprintingBonus = value;
                                if (value > 0) cv.isBred = true;
                                break;
                            // Colorization
                            case "ColorSet[0]":
                                cv.colorIDs[0] = parseColor(text);
                                break;
                            case "ColorSet[1]":
                                cv.colorIDs[1] = parseColor(text);
                                break;
                            case "ColorSet[2]":
                                cv.colorIDs[2] = parseColor(text);
                                break;
                            case "ColorSet[3]":
                                cv.colorIDs[3] = parseColor(text);
                                break;
                            case "ColorSet[4]":
                                cv.colorIDs[4] = parseColor(text);
                                break;
                            case "ColorSet[5]":
                                cv.colorIDs[5] = parseColor(text);
                                break;
                            case "Health":
                                cv.statValues[0] = value;
                                break;
                            case "Stamina":
                                cv.statValues[1] = value;
                                break;
                            case "Torpidity":
                                cv.statValues[7] = value;
                                break;
                            case "Oxygen":
                                cv.statValues[2] = value;
                                break;
                            case "Food":
                                cv.statValues[3] = value;
                                break;
                            case "Weight":
                                cv.statValues[4] = value;
                                break;
                            case "Melee Damage":
                                cv.statValues[5] = 1 + value;
                                break;
                            case "Movement Speed":
                                cv.statValues[6] = 1 + value;
                                break;
                        }
                    }
                }
                else if (line.Contains("[Max Character Status Values]"))
                {
                    inStatSection = true;
                }
            }
            return cv;
        }

        private static int parseColor(string text)
        {
            double.TryParse(text.Substring(3, 8), out double r);
            double.TryParse(text.Substring(14, 8), out double g);
            double.TryParse(text.Substring(25, 8), out double b);
            return CreatureColors.closestColorIDFromRGB(LinearColorComponentToColorComponent(r),
                                                        LinearColorComponentToColorComponent(g),
                                                        LinearColorComponentToColorComponent(b));
        }

        private static int LinearColorComponentToColorComponent(double lc)
        {
            return (int)(255.999f * (lc <= 0.0031308f ? lc * 12.92f : Math.Pow(lc, 1.0f / 2.4f) * 1.055f - 0.055f));
        }

        private static Guid builtGuid(string id1, string id2)
        {
            int.TryParse(id1, out int id1int);
            int.TryParse(id2, out int id2int);
            return Utils.ConvertIdToGuid((((long)id1int) << 32) | (id2int & 0xFFFFFFFFL));
        }

    }
}
