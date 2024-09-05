using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using MattArtifacts.Artifacts;
using R2API;
using UnityEngine;

namespace MattArtifacts
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MattArtifacts : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "MateusZ3";
        public const string PluginName = "MattArtifacts";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle MainAssets;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();

        public static ManualLogSource ModLogger;

        public void Awake()
        {
            ModLogger = Logger;

            ModLogger.LogDebug("Loading");

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "MattArtifacts.mattartifacts_bundle";
                
                ModLogger.LogDebug(assembly.GetManifestResourceNames());
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        ModLogger.LogError($"Failed to find resource: {resourceName}");
                        return;
                    }

                    MainAssets = AssetBundle.LoadFromStream(stream);
                }
            }
            catch (Exception e)
            {
                ModLogger.LogFatal(e);
                throw;
            }

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true,
                "Should this artifact appear for selection?").Value;

            if (enabled)
            {
                artifactList.Add(artifact);
            }

            return enabled;
        }
    }
}