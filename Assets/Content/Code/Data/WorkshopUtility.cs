using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade
{
    public static class WorkshopUtility
    {
        private static StringBuilder sb = new StringBuilder ();

        public static string GetProjectName (DataContainerWorkshopProject project)
        {
            if (project == null)
                return string.Empty;

            string textName = string.Empty;

            if (project.textSourceName != null)
                textName = project.textSourceName.GetText ();

            /*
            if (!string.IsNullOrEmpty (project.textFromPartPreset))
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (project.textFromPartPreset, false);
                if (partPreset != null)
                {
                    partPreset = DataHelperEquipment.GetLastInheritor (partPreset);
                    textName = partPreset.GetPartModelName ();
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromSubsystem))
            {
                var subsystem = DataMultiLinkerSubsystem.GetEntry (project.textFromSubsystem, false);
                if (subsystem != null)
                {
                    textName = subsystem.textNameProcessed?.s;
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromGroup))
            {
                var group = DataMultiLinkerEquipmentGroup.GetEntry (project.textFromGroup, false);
                if (group != null)
                {
                    if (!string.IsNullOrEmpty (group.textName))
                        textName = group.textName;
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromHardpoint))
            {
                var hardpoint = DataMultiLinkerSubsystemHardpoint.GetEntry (project.textFromHardpoint, false);
                if (hardpoint != null)
                {
                    if (!string.IsNullOrEmpty (hardpoint.textName))
                        textName = hardpoint.textName;
                }
            }
            */

            if (string.IsNullOrEmpty (textName))
                textName = project.textName;

            return textName;
        }

        public static string GetProjectSubtitle (DataContainerWorkshopProject project)
        {
            if (project == null)
                return string.Empty;

            string textSubtitle = string.Empty;

            if (project.textSourceSubtitle != null)
                textSubtitle = project.textSourceSubtitle.GetText ();

            /*
            if (!string.IsNullOrEmpty (project.textFromPartPreset))
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (project.textFromPartPreset, false);
                if (partPreset != null)
                {
                    partPreset = DataHelperEquipment.GetLastInheritor (partPreset);
                    if (!string.IsNullOrEmpty (partPreset.groupMainKey))
                    {
                        var group = DataMultiLinkerEquipmentGroup.GetEntry (partPreset.groupMainKey, false);
                        if (group != null && !string.IsNullOrEmpty (group.textName))
                            textSubtitle = group.textName;
                    }
                }
            }
            if (!string.IsNullOrEmpty (project.textFromSubsystem))
            {
                var subsystem = DataMultiLinkerSubsystem.GetEntry (project.textFromSubsystem, false);
                if (subsystem != null)
                {
                    // subsystem = DataHelperEquipment.GetLastInheritor (subsystem);
                    var hardpointInfo = EquipmentUtility.GetHardpointInfoForSubsystem (subsystem);
                    if (hardpointInfo != null)
                        textSubtitle = hardpointInfo.textName;
                }
            }
            */

            return textSubtitle;
        }

        public static string GetProjectDescription (DataContainerWorkshopProject project)
        {
            if (project == null)
                return string.Empty;

            var textDesc = string.Empty;

            if (project.textSourceDesc != null)
                textDesc = project.textSourceDesc.GetText ();

            /*
            if (!string.IsNullOrEmpty (project.textFromPartPreset))
            {
                var partPreset = DataMultiLinkerPartPreset.GetEntry (project.textFromPartPreset, false);
                if (partPreset != null)
                {
                    var desc = DataHelperEquipment.GetPartModelDesc (partPreset);
                    if (!string.IsNullOrEmpty (desc))
                        sb.Append (desc);
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromSubsystem))
            {
                var subsystem = DataMultiLinkerSubsystem.GetEntry (project.textFromSubsystem, false);
                if (subsystem != null)
                {
                    // subsystem = DataHelperEquipment.GetLastInheritor (subsystem);
                    var hardpointInfo = EquipmentUtility.GetHardpointInfoForSubsystem (subsystem);
                    if (hardpointInfo != null && !string.IsNullOrEmpty (hardpointInfo.textDesc))
                        sb.Append (hardpointInfo.textDesc);
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromGroup))
            {
                var group = DataMultiLinkerEquipmentGroup.GetEntry (project.textFromGroup, false);
                if (group != null)
                {
                    if (!string.IsNullOrEmpty (group.textDesc))
                        sb.Append (group.textDesc);
                }
            }
            else if (!string.IsNullOrEmpty (project.textFromHardpoint))
            {
                var hardpoint = DataMultiLinkerSubsystemHardpoint.GetEntry (project.textFromHardpoint, false);
                if (hardpoint != null)
                {
                    if (!string.IsNullOrEmpty (hardpoint.textDesc))
                        sb.Append (hardpoint.textDesc);
                }
            }
            */

            // Only pull workshop specific description if none of the above produced a description
            if (string.IsNullOrEmpty (textDesc))
                textDesc = project.textDesc;

            return textDesc;
        }

        private static void ApplyVariant (DataBlockWorkshopProjectProcessed p, DataBlockWorkshopProjectVariant variant)
        {
            if (p == null || variant == null)
                return;

            if (variant.ratingOverride != null)
                p.rating = Mathf.Max (variant.ratingOverride.i, 0);

            if (variant.durationMultiplier != null)
                p.duration *= variant.durationMultiplier.f;

            if (DataShortcuts.sim.workshopChargeSpending && variant.inputChargeMultiplier != null)
                p.inputCharges *= Mathf.RoundToInt (variant.inputChargeMultiplier.f);

            if (inputResourcesTemp.Count > 0 && variant.inputResourceMultipliers != null)
            {
                foreach (var kvp in variant.inputResourceMultipliers)
                {
                    if (inputResourcesTemp.ContainsKey (kvp.Key))
                    {
                        var f = inputResourcesTemp[kvp.Key];
                        inputResourcesTemp[kvp.Key] = f * kvp.Value;
                    }
                }
            }

            if (variant.basePartRequirements != null && variant.basePartRequirements.Count > 0)
            {
                foreach (var kvp in variant.basePartRequirements)
                {
                    if (!p.basePartRequirements.ContainsKey (kvp.Key))
                        p.basePartRequirements.Add (kvp.Key, kvp.Value);
                    else if (p.basePartRequirements[kvp.Key] < kvp.Value)
                        p.basePartRequirements[kvp.Key] = kvp.Value;
                }
            }

            if (variant.baseStatRequirements != null && variant.baseStatRequirements.Count > 0)
            {
                foreach (var kvp in variant.baseStatRequirements)
                {
                    if (!p.baseStatRequirements.ContainsKey (kvp.Key))
                        p.baseStatRequirements.Add (kvp.Key, kvp.Value);
                    else
                        p.baseStatRequirements[kvp.Key] = kvp.Value;
                }
            }

            if (p.outputUnits != null && p.outputUnits.Count > 0)
            {
                foreach (var outputUnit in p.outputUnits)
                {
                    if (outputUnit == null)
                        continue;

                    if (variant.outputUnitTags != null)
                    {
                        if (outputUnit.tags == null)
                            outputUnit.tags = new SortedDictionary<string, bool> ();

                        foreach (var kvp in variant.outputUnitTags)
                        {
                            if (outputUnit.tags.ContainsKey (kvp.Key))
                                outputUnit.tags[kvp.Key] = kvp.Value;
                            else
                                outputUnit.tags.Add (kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            if (p.outputParts != null && p.outputParts.Count > 0)
            {
                foreach (var outputPart in p.outputParts)
                {
                    if (outputPart == null)
                        continue;

                    if (variant.outputPartTags != null)
                    {
                        if (outputPart.tags == null)
                            outputPart.tags = new SortedDictionary<string, bool> ();

                        foreach (var kvp in variant.outputPartTags)
                        {
                            if (outputPart.tags.ContainsKey (kvp.Key))
                                outputPart.tags[kvp.Key] = kvp.Value;
                            else
                                outputPart.tags.Add (kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            if (p.outputSubsystems != null && p.outputSubsystems.Count > 0)
            {
                foreach (var outputSubsystem in p.outputSubsystems)
                {
                    if (outputSubsystem == null)
                        continue;

                    if (variant.outputSubsystemTags != null)
                    {
                        if (outputSubsystem.tags == null)
                            outputSubsystem.tags = new SortedDictionary<string, bool> ();

                        foreach (var kvp in variant.outputSubsystemTags)
                        {
                            if (outputSubsystem.tags.ContainsKey (kvp.Key))
                                outputSubsystem.tags[kvp.Key] = kvp.Value;
                            else
                                outputSubsystem.tags.Add (kvp.Key, kvp.Value);
                        }
                    }
                }
            }
        }

        private static SortedDictionary<string, float> inputResourcesTemp = new SortedDictionary<string, float> ();

        public static void ProcessProject (this DataBlockWorkshopProjectProcessed p, DataContainerWorkshopProject project, string variantPrimaryKey = null, string variantSecondaryKey = null)
        {
            if (project == null || p == null)
                return;

            inputResourcesTemp.Clear ();

            p.projectKey = project.key;
            p.variantPrimaryKey = variantPrimaryKey;
            p.variantSecondaryKey = variantSecondaryKey;

            p.rating = 1;
            p.duration = 0f;
            p.inputCharges = 0;
            p.inputResources.Clear ();
            p.basePartRequirements.Clear ();
            p.outputSubsystems.Clear ();
            p.outputParts.Clear ();
            p.outputUnits.Clear ();
            p.outputResources.Clear ();

            p.duration = project.duration != null ? project.duration.f : 0f;
            p.inputCharges = DataShortcuts.sim.workshopChargeSpending && project.inputCharges != null ? project.inputCharges.i : 0;

            if (project.basePartRequirements != null && project.basePartRequirements.Count > 0)
            {
                foreach (var kvp in project.basePartRequirements)
                    p.basePartRequirements.Add (kvp.Key, kvp.Value);
            }

            if (project.baseStatRequirements != null && project.baseStatRequirements.Count > 0)
            {
                foreach (var kvp in project.baseStatRequirements)
                    p.baseStatRequirements.Add (kvp.Key, kvp.Value);
            }

            if (project.inputResources != null && project.inputResources.Count > 0)
            {
                for (int i = 0, count = project.inputResources.Count; i < count; ++i)
                {
                    var block = project.inputResources[i];
                    var resource = DataMultiLinkerResource.GetEntry (block.key);
                    if (resource == null)
                        continue;

                    var amountFinal = block.amount;
                    if (inputResourcesTemp.ContainsKey (block.key))
                        inputResourcesTemp[block.key] += amountFinal;
                    else
                        inputResourcesTemp.Add (block.key, amountFinal);
                }
            }

            if (project.outputUnits != null && project.outputUnits.Count > 0)
            {
                if (project.outputUnits.Count != p.outputUnits.Count)
                {
                    foreach (var outputUnit in project.outputUnits)
                    {
                        p.outputUnits.Add (new DataBlockWorkshopUnit
                        {
                            factionBranch = outputUnit.factionBranch,
                            qualityTable = outputUnit.qualityTable,
                            key = outputUnit.key,
                            tagsUsed = outputUnit.tagsUsed,
                            tags = outputUnit.tags != null ? new SortedDictionary<string, bool> (outputUnit.tags) : null
                        });
                    }
                }
                else
                {
                    for (int i = 0, count = project.outputUnits.Count; i < count; ++i)
                    {
                        var outputUnit = project.outputUnits[i];
                        var o = p.outputUnits[i];

                        o.factionBranch = outputUnit.factionBranch;
                        o.qualityTable = outputUnit.qualityTable;
                        o.key = outputUnit.key;
                        o.tagsUsed = outputUnit.tagsUsed;

                        if (o.tagsUsed && outputUnit.tags != null)
                        {
                            if (o.tags == null)
                                o.tags = new SortedDictionary<string, bool> (outputUnit.tags);
                            else
                            {
                                o.tags.Clear ();
                                foreach (var kvp in outputUnit.tags)
                                    o.tags.Add (kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
            }

            if (project.outputParts != null && project.outputParts.Count > 0)
            {
                if (project.outputParts.Count != p.outputParts.Count)
                {
                    foreach (var outputPart in project.outputParts)
                    {
                        p.outputParts.Add (new DataBlockWorkshopPart
                        {
                            count = outputPart.count,
                            key = outputPart.key,
                            tags = outputPart.tags != null ? new SortedDictionary<string, bool> (outputPart.tags) : null
                        });
                    }
                }
                else
                {
                    for (int i = 0, count = project.outputParts.Count; i < count; ++i)
                    {
                        var outputPart = project.outputParts[i];
                        var o = p.outputParts[i];

                        o.count = outputPart.count;
                        o.key = outputPart.key;

                        if (outputPart.tags != null)
                        {
                            if (o.tags == null)
                                o.tags = new SortedDictionary<string, bool> (outputPart.tags);
                            else
                            {
                                o.tags.Clear ();
                                foreach (var kvp in outputPart.tags)
                                    o.tags.Add (kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
            }

            if (project.outputSubsystems != null && project.outputSubsystems.Count > 0)
            {
                if (project.outputSubsystems.Count != p.outputSubsystems.Count)
                {
                    p.outputSubsystems.Clear ();
                    foreach (var outputSubsystem in project.outputSubsystems)
                    {
                        p.outputSubsystems.Add (new DataBlockWorkshopSubsystem
                        {
                            count = outputSubsystem.count,
                            key = outputSubsystem.key,
                            tagsUsed = outputSubsystem.tagsUsed,
                            tags = outputSubsystem.tags != null ? new SortedDictionary<string, bool> (outputSubsystem.tags) : null
                        });
                    }
                }
                else
                {
                    for (int i = 0, count = project.outputSubsystems.Count; i < count; ++i)
                    {
                        var outputSubsystem = project.outputSubsystems[i];
                        var o = p.outputSubsystems[i];

                        o.count = outputSubsystem.count;
                        o.key = outputSubsystem.key;
                        o.tagsUsed = outputSubsystem.tagsUsed;

                        if (o.tagsUsed && outputSubsystem.tags != null)
                        {
                            if (o.tags == null)
                                o.tags = new SortedDictionary<string, bool> (outputSubsystem.tags);
                            else
                            {
                                o.tags.Clear ();
                                foreach (var kvp in outputSubsystem.tags)
                                    o.tags.Add (kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
            }

            if (project.outputResources != null)
            {
                p.outputResources.Clear ();
                foreach (var output in project.outputResources)
                {
                    p.outputResources.Add (new DataBlockWorkshopOutputResource
                    {
                        key = output.key,
                        amount = output.amount
                    });
                }
            }

            var variantsPrimary = project.GetVariantsPrimary ();
            bool variantPrimaryUsed = !string.IsNullOrEmpty (variantPrimaryKey);
            if (variantPrimaryUsed)
            {
                if (variantsPrimary != null && variantsPrimary.ContainsKey (variantPrimaryKey) && variantsPrimary[variantPrimaryKey] != null)
                {
                    var variantPrimary = variantPrimaryUsed ? variantsPrimary[variantPrimaryKey] : null;
                    ApplyVariant (p, variantPrimary);
                }
            }

            var variantsSecondary = project.GetVariantsSecondary ();
            bool variantSecondaryUsed = !string.IsNullOrEmpty (variantSecondaryKey);
            if (variantSecondaryUsed)
            {
                if (variantsSecondary != null && variantsSecondary.ContainsKey (variantSecondaryKey) && variantsSecondary[variantSecondaryKey] != null)
                {
                    var variantSecondary = variantSecondaryUsed ? variantsSecondary[variantSecondaryKey] : null;
                    ApplyVariant (p, variantSecondary);
                }
            }

            if (inputResourcesTemp.Count > 0)
            {
                foreach (var kvp in inputResourcesTemp)
                {
                    var amountFinal = Mathf.RoundToInt (kvp.Value);
                    if (amountFinal > 0)
                        p.inputResources.Add (kvp.Key, amountFinal);
                }
            }
        }

        public static bool AreVariantInputsValid (DataContainerWorkshopProject project, string variantPrimaryKey, string variantSecondaryKey)
        {
            if (project == null)
                return false;

            bool variantPrimaryUsed = !string.IsNullOrEmpty (variantPrimaryKey);
            if (variantPrimaryUsed)
            {
                var variantsPrimary = project.GetVariantsPrimary ();
                if (variantsPrimary == null || !variantsPrimary.ContainsKey (variantPrimaryKey) || variantsPrimary[variantPrimaryKey] == null)
                {
                    Debug.LogWarning ($"Primary variant key {variantPrimaryKey} is not valid for workshop project {project.key}, can't start build");
                    return false;
                }
            }

            bool variantSecondaryUsed = !string.IsNullOrEmpty (variantSecondaryKey);
            if (variantSecondaryUsed)
            {
                var variantsSecondary = project.GetVariantsSecondary ();
                if (variantsSecondary == null || !variantsSecondary.ContainsKey (variantSecondaryKey) || variantsSecondary[variantSecondaryKey] == null)
                {
                    Debug.LogWarning ($"Secondary variant key {variantSecondaryKey} is not valid for workshop project {project.key}, can't start build");
                    return false;
                }
            }

            return true;
        }


        public static string GetPackedArgument (string projectKey, string variantPrimaryKey, string variantSecondaryKey)
        {
            var packedArgProject = projectKey;
            var packedArgVariantPrimary = !string.IsNullOrEmpty (variantPrimaryKey) ? variantPrimaryKey : string.Empty;
            var packedArgVariantSecondary = !string.IsNullOrEmpty (variantSecondaryKey) ? variantSecondaryKey : string.Empty;
            return $"{packedArgProject},{packedArgVariantPrimary},{packedArgVariantSecondary}";
        }

        public static void GetUnpackedArguments (string packedArg, out string projectKey, out string variantPrimaryKey, out string variantSecondaryKey)
        {
            projectKey = null;
            variantPrimaryKey = null;
            variantSecondaryKey = null;

            var split = packedArg.Split (',');
            var count = split.Length;

            if (count > 0)
                projectKey = split[0];

            if (count > 1)
                variantPrimaryKey = split[1];

            if (count > 2)
                variantSecondaryKey = split[2];
        }
    }
}

