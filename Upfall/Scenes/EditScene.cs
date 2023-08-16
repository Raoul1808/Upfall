using System;
using System.IO;
using Brocco;
using Brocco.Basic;
using Brocco.Input;
using Brocco.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NativeFileDialogSharp;

namespace Upfall.Scenes;

public class EditScene : Scene
{
    private Tilemap _tilemap;
    private Size _tilemapSize;
    private Point _currentTilePos;
    private string _levelFilename = string.Empty;

    private MenuObject _editorMenu;
    private bool _showEditorMenu = false;

    private TileType _currentTileId;
    private Direction _currentDirection;
    
    public override void Load()
    {
        ClearColor = Color.CornflowerBlue;
        PauseUpdate = true;
        var menuSettings = new MenuSettings
        {
            FontSize = 32,
        };
        _editorMenu = MenuBuilder.CreateMenu(Assets.GetFontSystem("Open Sans"), new Vector2(200), menuSettings)
            .AddButton("Resume", _ => _showEditorMenu = false)
            .AddTextInput("Level Name")
            .AddButton("Exit to Menu", _ =>
            {
                SceneManager.Change("Menu");
                _showEditorMenu = false;
            })
            .Build();
    }

    private void CreateBlankTilemap()
    {
        _tilemap = new Tilemap(_tilemapSize = new Size(40, 23));
        _levelFilename = string.Empty;
    }

    public override void OnBecomeActive()
    {
        if (!UpfallCommon.Playtesting)
            CreateBlankTilemap();
        UpfallCommon.CurrentWorldMode = WorldMode.None;
        UpfallCommon.InEditor = true;
        UpfallCommon.Playtesting = false;
        _currentTileId = TileType.Solid;
        _currentDirection = Direction.Up;
    }

    public override void OnBecomeInactive()
    {
        UpfallCommon.InEditor = false;
        UpfallCommon.Playtesting = true;
    }

    public override void Update(float dt)
    {
        if (InputManager.GetKeyPress(Keys.Escape))
            _showEditorMenu = !_showEditorMenu;
        
        if (_showEditorMenu)
        {
            _editorMenu.Update();
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
                DialogResult res;
                if ((_levelFilename == string.Empty || InputManager.GetKeyDown(Keys.LeftShift)) &&
                    (res = Dialog.FileSave("umd", UpfallCommon.GamePath)).IsOk)
                {
                    _levelFilename = res.Path;
                }

                if (_levelFilename == string.Empty)
                    NotificationSystem.SendNotification("Cannot save: path not set");
                else
                {
                    _tilemap.SaveToFile(_levelFilename);
                    NotificationSystem.SendNotification("Saved Level to " + _levelFilename);
                }
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

            if (InputManager.GetKeyPress(Keys.T))
            {
                _tilemap.SaveToFile("map.umd");
                SceneManager.Change("Game");
                NotificationSystem.SendNotification("Now playing level " + _levelFilename);
                return;  // Don't execute further
            }

            if (InputManager.GetKeyPress(Keys.N))
            {
                CreateBlankTilemap();
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
            case WorldMode.None:
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
            _editorMenu.Render(spriteBatch);
    }
}
