using System;
using System.IO;
using Brocco;
using Brocco.Basic;
using Brocco.Input;
using Brocco.Menu;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NativeFileDialogSharp;

namespace Upfall.Scenes;

public class EditScene : Scene
{
    private enum MenuState
    {
        Main,
        SimplePalette,
        LerpPalette,
    }
    
    private Tilemap _tilemap;
    private Size _tilemapSize;
    private Point _currentTilePos;
    private static string _levelFilename = string.Empty;

    public static string TilemapToLoad => _levelFilename;

    private MenuObject _editorMenu;
    private MenuObject _simplePaletteMenu;
    private MenuObject _lerpPaletteMenu;
    private MenuState _currentMenu;
    private bool _showEditorMenu = false;

    private PaletteType _currentPaletteType;
    private string _simpleDarkCol;
    private string _simpleLightCol;
    private string _lerpDarkCol1;
    private string _lerpDarkCol2;
    private string _lerpLightCol1;
    private string _lerpLightCol2;

    private TileType _currentTileId;
    private Direction _currentDirection;

    private MenuObject GetCurrentMenu()
    {
        return _currentMenu switch
        {
            MenuState.Main => _editorMenu,
            MenuState.SimplePalette => _simplePaletteMenu,
            MenuState.LerpPalette => _lerpPaletteMenu,
            _ => null,
        };
    }
    
    public override void Load()
    {
        ClearColor = Color.CornflowerBlue;
        PauseUpdate = true;
    }

    private void OnConfigurePalettePressed(MenuButton sender)
    {
        switch (_currentPaletteType)
        {
            case PaletteType.Simple:
                _currentMenu = MenuState.SimplePalette;
                break;

            case PaletteType.Lerp:
                _currentMenu = MenuState.LerpPalette;
                break;

            case PaletteType.Trippy:
                NotificationSystem.SendNotification("No configuration available for Trippy Palette");
                break;
        }
    }

    private void OnPaletteTypeChange(MenuArraySelect<PaletteType> sender, PaletteType selectedOption)
    {
        _currentPaletteType = selectedOption;
        switch (selectedOption)
        {
            case PaletteType.Simple:
                NotificationSystem.SendNotification("Remember to configure your simple palette!");
                break;

            case PaletteType.Lerp:
                NotificationSystem.SendNotification("Remember to configure your lerp palette!");
                break;

            case PaletteType.Trippy:
                NotificationSystem.SendNotification("Level Palette Selected: Trippy");
                _tilemap.LevelPalette = new TrippyPalette();
                break;
        }
    }

    private void CreateBlankTilemap()
    {
        _tilemap = new Tilemap(_tilemapSize = new Size(40, 23));
        _levelFilename = string.Empty;
    }

    public override void OnBecomeActive()
    {
        PaletteSystem.ResetPalette(0f);
        if (!UpfallCommon.Playtesting)
            RefreshEditor();
        UpfallCommon.CurrentWorldMode = WorldMode.Common;
        UpfallCommon.InEditor = true;
        UpfallCommon.Playtesting = false;
        _currentTileId = TileType.Solid;
        _currentDirection = Direction.Up;
    }

    private void RefreshEditor(bool createTilemap = true)
    {
        if (createTilemap)
            CreateBlankTilemap();
        _simpleDarkCol = "000000";
        _simpleLightCol = "ffffff";
        _lerpDarkCol1 = "000000";
        _lerpDarkCol2 = "000000";
        _lerpLightCol1 = "ffffff";
        _lerpLightCol2 = "ffffff";
        _currentPaletteType = _tilemap.LevelPalette.PaletteType;
        
        if (_tilemap.LevelPalette is SimplePalette simple)
        {
            _simpleDarkCol = ColorUtil.ColToHex(simple.DarkColor);
            _simpleLightCol = ColorUtil.ColToHex(simple.LightColor);
        }

        if (_tilemap.LevelPalette is LerpPalette lerp)
        {
            _lerpDarkCol1 = ColorUtil.ColToHex(lerp.DarkColor1);
            _lerpDarkCol2 = ColorUtil.ColToHex(lerp.DarkColor2);
            _lerpLightCol1 = ColorUtil.ColToHex(lerp.LightColor1);
            _lerpLightCol2 = ColorUtil.ColToHex(lerp.LightColor2);
        }
        
        var menuSettings = new MenuSettings
        {
            FontSize = 32,
            FontEffect = FontSystemEffect.Stroked,
            FontEffectStrength = 1,
        };
        
        var openSans = Assets.GetFontSystem("Open Sans");
        
        _editorMenu = MenuBuilder.CreateMenu(openSans, UpfallCommon.ScreenCenter, menuSettings)
            .AddButton("Resume", _ => _showEditorMenu = false)
            .AddTextInput("Level Name", _tilemap.LevelName, (_, name) => _tilemap.LevelName = name)
            .AddTextInput("Level Author", _tilemap.LevelAuthor, (_, name) => _tilemap.LevelAuthor = name)
            .AddArraySelect("Level Palette Type", Enum.GetValues<PaletteType>(), (int)_currentPaletteType, OnPaletteTypeChange)
            .AddButton("Configure Palette", OnConfigurePalettePressed)
            .AddButton("Exit to Menu", _ =>
            {
                _showEditorMenu = false;
                SceneManager.Change("Menu");
            })
            .Build();

        _simplePaletteMenu = MenuBuilder.CreateMenu(openSans, UpfallCommon.ScreenCenter, menuSettings)
            .AddTextInput("Dark Color Hex Code", _simpleDarkCol, (_, str) => _simpleDarkCol = str)
            .AddTextInput("Light Color Hex Code", _simpleLightCol, (_, str) => _simpleLightCol = str)
            .AddButton("Apply Changes", _ => CreateSimplePalette())
            .AddButton("Back", _ => _currentMenu = MenuState.Main)
            .Build();

        _lerpPaletteMenu = MenuBuilder.CreateMenu(openSans, UpfallCommon.ScreenCenter, menuSettings)
            .AddTextInput("Dark Color 1 Hex Code", _lerpDarkCol1, (_, str) => _lerpDarkCol1 = str)
            .AddTextInput("Dark Color 2 Hex Code", _lerpDarkCol2, (_, str) => _lerpDarkCol2 = str)
            .AddTextInput("Light Color 1 Hex Code", _lerpLightCol1, (_, str) => _lerpLightCol1 = str)
            .AddTextInput("Light Color 2 Hex Code", _lerpLightCol2, (_, str) => _lerpLightCol2 = str)
            .AddButton("Apply Changes", _ => CreateLerpPalette())
            .AddButton("Back", _ => _currentMenu = MenuState.Main)
            .Build();
    }

    private void CreateSimplePalette()
    {
        string darkHex = _simpleDarkCol.ToLower();
        if (!ColorUtil.IsValidRgbHex(darkHex))
        {
            NotificationSystem.SendNotification("Invalid hex color for Dark Color");
            return;
        }

        Color colA = ColorUtil.HexToCol(darkHex);

        string lightHex = _simpleLightCol.ToLower();
        if (!ColorUtil.IsValidRgbHex(lightHex))
        {
            NotificationSystem.SendNotification("Invalid hex color for Light Color");
            return;
        }

        Color colB = ColorUtil.HexToCol(lightHex);

        var palette = new SimplePalette
        {
            DarkColor = colA,
            LightColor = colB,
        };
        _tilemap.LevelPalette = palette;
        NotificationSystem.SendNotification("Successfully applied simple palette");
    }

    private void CreateLerpPalette()
    {
        string darkHex1 = _lerpDarkCol1.ToLower();
        if (!ColorUtil.IsValidRgbHex(darkHex1))
        {
            NotificationSystem.SendNotification("Invalid hex color for Dark Color 1");
            return;
        }
        Color colA = ColorUtil.HexToCol(darkHex1);
        
        string darkHex2 = _lerpDarkCol2.ToLower();
        if (!ColorUtil.IsValidRgbHex(darkHex2))
        {
            NotificationSystem.SendNotification("Invalid hex color for Dark Color 2");
            return;
        }
        Color colB = ColorUtil.HexToCol(darkHex2);

        string lightHex1 = _lerpLightCol1.ToLower();
        if (!ColorUtil.IsValidRgbHex(lightHex1))
        {
            NotificationSystem.SendNotification("Invalid hex color for Light Color 1");
            return;
        }
        Color colC = ColorUtil.HexToCol(lightHex1);

        string lightHex2 = _lerpLightCol2.ToLower();
        if (!ColorUtil.IsValidRgbHex(lightHex2))
        {
            NotificationSystem.SendNotification("Invalid hex color for Light Color 2");
            return;
        }
        Color colD = ColorUtil.HexToCol(lightHex2);

        var palette = new LerpPalette()
        {
            DarkColor1 = colA,
            DarkColor2 = colB,
            LightColor1 = colC,
            LightColor2 = colD,
        };
        _tilemap.LevelPalette = palette;
        NotificationSystem.SendNotification("Successfully applied lerp palette");
    }

    public override void OnBecomeInactive()
    {
        UpfallCommon.InEditor = false;
        if (UpfallCommon.Playtesting)
            PaletteSystem.SetPalette(_tilemap.LevelPalette);
    }

    private void SaveToNewLocation()
    {
        DialogResult res = Dialog.FileSave("umd", UpfallCommon.CustomLevelsPath);
        if (res.IsOk)
        {
            _levelFilename = res.Path;
            SaveLevel();
        }
    }

    private bool SaveLevel()
    {
        if (_levelFilename == string.Empty)
        {
            NotificationSystem.SendNotification("Cannot save: path not set");
            return false;
        }

        if (!_levelFilename.EndsWith(".umd"))
            _levelFilename += ".umd";

        _tilemap.SaveToFile(_levelFilename);
        NotificationSystem.SendNotification("Saved Level to " + _levelFilename);
        return true;
    }

    public override void Update(float dt)
    {
        if (InputManager.GetKeyPress(Keys.Escape))
            _showEditorMenu = !_showEditorMenu;
        
        if (_showEditorMenu)
        {
            GetCurrentMenu()?.Update();
            return;
        }

        if (InputManager.GetKeyPress(Keys.Tab))
        {
            UpfallCommon.CycleWorldMode();
            NotificationSystem.SendNotification("Tilemap Selected: " + UpfallCommon.CurrentWorldMode);
        }
        
        var pos = InputManager.GetCanvasMousePosition();
        int x = Math.Clamp((int)pos.X / Tilemap.TileSize, 0, _tilemapSize.Width - 1);
        int y = Math.Clamp((int)pos.Y / Tilemap.TileSize, 0, _tilemapSize.Height - 1);
        _currentTilePos = new Point(x, y);
        if (InputManager.GetClickDown(MouseButtons.Left))
            SetTile(_currentTilePos, _currentTileId, _currentDirection);
        if (InputManager.GetClickDown(MouseButtons.Right))
            SetTile(_currentTilePos, TileType.None, _currentDirection);
        if (InputManager.GetClickDown(MouseButtons.Middle))
        {
            if (InputManager.GetKeyDown(Keys.LeftControl))
            {
                _tilemap.SetExit(_currentTilePos);
            }
            else
            {
                _tilemap.SetSpawn(_currentTilePos);
            }
        }

        if (InputManager.GetKeyDown(Keys.LeftControl))
        {
            if (InputManager.GetKeyPress(Keys.S))
            {
                if (_levelFilename == string.Empty || InputManager.GetKeyDown(Keys.LeftShift))
                    SaveToNewLocation();
                else
                    SaveLevel();
            }

            if (InputManager.GetKeyPress(Keys.O))
            {
                DialogResult res = Dialog.FileOpen("umd", UpfallCommon.GamePath);
                if (!res.IsOk)
                    NotificationSystem.SendNotification("Open File Request Cancelled.");
                try
                {
                    var tilemap = Tilemap.LoadFromFile(res.Path);
                    _tilemap = tilemap;
                    NotificationSystem.SendNotification("Loaded Level from " + res.Path);
                    _levelFilename = res.Path;
                    RefreshEditor(false);
                }
                catch (IOException)
                {
                    NotificationSystem.SendNotification("Level " + _levelFilename + " doesn't exist");
                }
                catch (Exception)
                {
                    NotificationSystem.SendNotification("Not a valid map");
                }
            }

            if (InputManager.GetKeyPress(Keys.T) && SaveLevel())
            {
                UpfallCommon.Playtesting = true;
                NotificationSystem.SendNotification("Now playing level " + _levelFilename);
                SceneManager.Change("Game");
                return;  // Don't execute further
            }

            if (InputManager.GetKeyPress(Keys.N))
            {
                RefreshEditor(true);
                NotificationSystem.SendNotification("Creating new level");
            }
        }

        if (InputManager.GetKeyPress(Keys.D1))
        {
            _currentTileId = TileType.Solid;
            NotificationSystem.SendNotification("Selected Tile: Solid");
        }

        if (InputManager.GetKeyPress(Keys.D2))
        {
            _currentTileId = TileType.Spike;
            NotificationSystem.SendNotification("Selected Tile: Spike");
        }

        if (InputManager.GetKeyPress(Keys.D3))
        {
            _currentTileId = TileType.Portal;
            NotificationSystem.SendNotification("Selected Tile: Portal");
        }

        if (InputManager.GetKeyPress(Keys.D4))
        {
            _currentTileId = TileType.Key;
            NotificationSystem.SendNotification("Selected Tile: Key");
        }

        if (InputManager.GetKeyPress(Keys.D5))
        {
            _currentTileId = TileType.Spawn;
            NotificationSystem.SendNotification("Selected Tile: Spawn");
        }

        if (InputManager.GetKeyPress(Keys.D6))
        {
            _currentTileId = TileType.ExitDoor;
            NotificationSystem.SendNotification("Selected Tile: Exit Door");
        }

        if (InputManager.GetKeyPress(Keys.Up))
        {
            _currentDirection = Direction.Up;
            NotificationSystem.SendNotification("Tile Facing: Up");
        }

        if (InputManager.GetKeyPress(Keys.Down))
        {
            _currentDirection = Direction.Down;
            NotificationSystem.SendNotification("Tile Facing: Down");
        }

        if (InputManager.GetKeyPress(Keys.Left))
        {
            _currentDirection = Direction.Left;
            NotificationSystem.SendNotification("Tile Facing: Left");
        }

        if (InputManager.GetKeyPress(Keys.Right))
        {
            _currentDirection = Direction.Right;
            NotificationSystem.SendNotification("Tile Facing: Right");
        }
    }

    private void SetTile(Point pos, TileType tile, Direction direction)
    {
        switch (tile)
        {
            case TileType.Spawn:
                _tilemap.SetSpawn(pos);
                return;
            
            case TileType.ExitDoor:
                _tilemap.SetExit(pos);
                return;
        }

        if (tile == TileType.Solid)
            direction = Direction.Right;
        
        switch (UpfallCommon.CurrentWorldMode)
        {
            case WorldMode.Common:
                _tilemap.SetCommonTile(pos, tile, direction);
                break;

            case WorldMode.Dark:
                _tilemap.SetDarkTile(pos, tile, direction);
                break;

            case WorldMode.Light:
                _tilemap.SetLightTile(pos, tile, direction);
                break;
        }
    }

    public override void CanvasRender(SpriteBatch spriteBatch)
    {
        _tilemap.EditorRender(spriteBatch);
        _tilemap.RenderTile(spriteBatch, Color.DimGray * 0.5f, _currentTilePos, _currentTileId, _currentDirection);
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        if (_showEditorMenu)
            GetCurrentMenu()?.Render(spriteBatch);
    }
}
