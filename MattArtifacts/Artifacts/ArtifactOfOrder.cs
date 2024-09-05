using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static MattArtifacts.MattArtifacts;

namespace MattArtifacts.Artifacts
{
    class ArtifactOfOrder : ArtifactBase
    {
        public static ConfigEntry<int> TimesToPrintMessageOnStart;
        public override string ArtifactName => "Artifact of Order";
        public override string ArtifactLangTokenName => "ARTIFACT_OF_ORDER";

        public override string ArtifactDescription =>
            "When enabled, print a message to the chat at the start of the run.";

        public override Sprite ArtifactEnabledIcon => MainAssets.LoadAsset<Sprite>("texOrderArtifactEnable.png");
        public override Sprite ArtifactDisabledIcon => MainAssets.LoadAsset<Sprite>("texOrderArtifactDisable.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            TimesToPrintMessageOnStart = config.Bind<int>("Artifact: " + ArtifactName, "Times to Print Message in Chat",
                5, "How many times should a message be printed to the chat on run start?");
        }

        public override void Hooks()
        {   
            // TODO: this needs to be set via config, must be one or the other
            On.RoR2.GenericPickupController.AttemptGrant += GenericPuckupController_AttemptGrant;
            On.RoR2.SceneExitController.Begin += SceneExitController_Begin;
        }

        private void GenericPuckupController_AttemptGrant(
            On.RoR2.GenericPickupController.orig_AttemptGrant orig,
            GenericPickupController self,
            RoR2.CharacterBody body)
        {
            orig(self, body);

            if (!(NetworkServer.active && ArtifactEnabled))
                return;


            var rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            body.inventory.ShrineRestackInventory(rng);
        }

        private void SceneExitController_Begin(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self)
        {


            if (!(NetworkServer.active && ArtifactEnabled))
            {
                orig(self);
                return;
            }
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                var rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
                player.body.inventory.ShrineRestackInventory(rng);
            }

            orig(self);
        }
    }
}