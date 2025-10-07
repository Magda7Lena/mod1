
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System.Collections.Generic;

namespace ClientSideBlockOverlay
{
    public class ClientSideBlockOverlay : ModSystem
    {
        private ICoreClientAPI capi;
        private bool modEnabled = true;
        private int searchDistance = 100;
        private KeyCombination toggleKey;
        private List<string> highlightBlocks = new List<string>();

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;

            // Rejestracja przycisku ON/OFF
            toggleKey = new KeyCombination(capi.Input.RegisterKeyBinding("clientOverlayToggle", "[", "ClientSide Overlay"));
            capi.Input.SetHotKeyHandler(toggleKey, ToggleMod);

            // Renderowanie overlay
            capi.Event.RegisterRenderer(Render, EnumRenderStage.Opaque, "clientSideOverlayRenderer");

            // Wczytanie konfiguracji z JSON
            LoadConfig();
        }

        private void ToggleMod()
        {
            modEnabled = !modEnabled;
            capi.Gui.ChatMessage("ClientSide Overlay: " + (modEnabled ? "ON" : "OFF"));
        }

        private void LoadConfig()
        {
            // Tutaj wczytujemy listę bloków z pliku ModConfig.json
            var cfg = capi.LoadModConfig<ModConfig>("ModConfig.json");
            if (cfg != null)
            {
                highlightBlocks = new List<string>(cfg.HighlightBlocks);
                searchDistance = cfg.SearchDistance;
            }
        }

        private void Render(float dt, EnumRenderStage stage)
        {
            if (!modEnabled) return;

            Vec3d playerPos = capi.World.Player.Entity.Pos.XYZ;
            int minX = (int)(playerPos.X - searchDistance);
            int maxX = (int)(playerPos.X + searchDistance);
            int minY = (int)(playerPos.Y - searchDistance);
            int maxY = (int)(playerPos.Y + searchDistance);
            int minZ = (int)(playerPos.Z - searchDistance);
            int maxZ = (int)(playerPos.Z + searchDistance);

            BlockPos blockPos = new BlockPos();
            List<BlockPos> highlightPositions = new List<BlockPos>();

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        blockPos.Set(x, y, z);
                        Block block = capi.World.BlockAccessor.GetBlock(blockPos);
                        if (block?.Code == null) continue;

                        foreach (var name in highlightBlocks)
                        {
                            if (block.Code.Path.Contains(name))
                            {
                                highlightPositions.Add(blockPos.Copy());
                                break;
                            }
                        }
                    }

            if (highlightPositions.Count > 0)
                capi.World.HighlightBlocks(capi.World.Player, highlightPositions);
        }
    }

    public class ModConfig
    {
        public List<string> HighlightBlocks { get; set; } = new List<string>();
        public int SearchDistance { get; set; } = 100;
    }
}
