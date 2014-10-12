
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using CollisionManager;
using SpriteAnimation;
using Box2D.XNA;

namespace OmegaRace
{
    public enum gameState
    {
        ready, // Flashes Ready? until the timer is up
        menu,
        game, // The main game mode
        pause,
        winner // Displays the winner
    };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }


        private static Game1 Game;
        public static Game1 GameInstance
        {
            get { return Game; }
        }

        private static Camera camera;
        public static Camera Camera
        {
            get { return camera; }
        }


        // Keyboard and Xbox Controller states
        KeyboardState oldState;
        KeyboardState newState;

        GamePadState P1oldPadState;
        GamePadState P1newPadState;

        GamePadState P2oldPadState;
        GamePadState P2newPadState;


        // For flipping game states
        public gameState state;


        // Box2D world
        World world;
        public World getWorld()
        {
            return world;
        }

        public Rectangle gameScreenSize;


        // Quick reference for Input 
        //Player player1;
        //Player player2;
        Player[] players;
        Player currentPlayer;
        // Max ship speed
        int shipSpeed;

        //Networking 
        public NetworkSession netSession;
        MenuScreen menu;
        public int maxLocalGamers = 2;
        public int maxGamers = 2;
        public LocalNetworkGamer gamer;
        public NetPredMan netPred;
        public static int netCount = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";


            graphics.PreferredBackBufferHeight = 500;
            graphics.PreferredBackBufferWidth = 800;

            gameScreenSize = new Rectangle(0, 0, 800, 500);

            state = gameState.ready;

            world = new World(new Vector2(0, 0), false);

            shipSpeed = 200;

            Game = this;
            gamer = null;
            // Add Gamer Services
            Components.Add(new GamerServicesComponent(this));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
           

            camera = new Camera(GraphicsDevice.Viewport, Vector2.Zero);

            state = gameState.menu;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

                world = new World(new Vector2(0, 0), false);

                myContactListener myContactListener = new myContactListener();

                world.ContactListener = myContactListener;


                Data.Instance().createData();

                state = gameState.menu;

                players = new Player[2];
                players[0] = PlayerManager.Instance().getPlayer(PlayerID.one);
                players[1] = PlayerManager.Instance().getPlayer(PlayerID.two);
                if (gamer != null && gamer == netSession.Host)
                {
                    currentPlayer = players[0];
                }
                else
                {
                    currentPlayer = players[1];
                }

                menu = new MenuScreen(this);
                netPred = new NetPredMan(this); 
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            GraphicsDevice.Clear(Color.Black);


            base.Update(gameTime);
            
            if (state == gameState.menu)
            {
                menu.update();
                if (netSession != null)
                {
                    netSession.Update();
                }
                Timer.Process(gameTime);
                
            }  

            if (state == gameState.game)
            {
                if (gamer.IsHost)
                {
                    world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 5, 8);
                    checkInput();
                    PhysicsMan.Instance().Update();

                }
                else
                {
                    checkInput();
                }


                OutputQueue.Instance.send((LocalNetworkGamer)gamer, this);
                InputQueue.Instance.readData((LocalNetworkGamer)gamer);
                InputQueue.Instance.process(this);

                netPred.update();
                GameObjManager.Instance().Update(world);
                
                ScoreManager.Instance().Update();             

                Timer.Process(gameTime);
                netSession.Update();
            }

            Game1.Camera.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (state == gameState.menu)
            {
                menu.draw();
            }
            else if (state == gameState.game)
            {
                SpriteBatchManager.Instance().process();
                netPred.draw();
                
            }


            base.Draw(gameTime);
        }

        public void GameOver()
        {
            state = gameState.winner;
 

            resetData();
        }

        public PlayerID getCurrentPlayerID()
        {
            return currentPlayer.id;
        }


        private void checkInput()
        {
            newState = Keyboard.GetState();
            P1newPadState = GamePad.GetState(PlayerIndex.One);
            P2newPadState = GamePad.GetState(PlayerIndex.Two);

            ShipInputMsg msg = new ShipInputMsg();
            bool send = false;

            if (oldState.IsKeyDown(Keys.D) || P1oldPadState.IsButtonDown(Buttons.DPadRight))
            {
                msg.pID = currentPlayer.id;
                msg.rot = .1f;
                send = true;
                //PlayerQueue.add(playerData);
            }

            if (oldState.IsKeyDown(Keys.A) || P1oldPadState.IsButtonDown(Buttons.DPadLeft))
            {
                msg.pID = currentPlayer.id;
                msg.rot = -.1f;
                send = true;
                //playerData.op = PlayerOp.rotate;
                //playerData.input = -.1f;
                //playerData.pIndex = 0;
                //playerData.pID = currentPlayer.id;
                //PlayerQueue.add(playerData);
            }

            if (oldState.IsKeyDown(Keys.W) || P1oldPadState.IsButtonDown(Buttons.DPadUp))
            {
                msg.pID = currentPlayer.id;
                Vector2 direction = new Vector2((float)(Math.Cos(currentPlayer.playerShip.physicsObj.body.GetAngle())), (float)(Math.Sin(currentPlayer.playerShip.physicsObj.body.GetAngle())));
                direction.Normalize();
                direction *= shipSpeed;
                msg.dir = direction;
                send = true;
                //playerData.op = PlayerOp.accelerate;
                //playerData.input = shipSpeed;
                //playerData.pIndex = 0;
                //playerData.pID = currentPlayer.id;
                //PlayerQueue.add(playerData);
            }

            if ((oldState.IsKeyDown(Keys.X) && newState.IsKeyUp(Keys.X)) || (P1oldPadState.IsButtonDown(Buttons.A) && P1newPadState.IsButtonUp(Buttons.A)))
            {
                if (currentPlayer.state == PlayerState.alive && currentPlayer.missileAvailable())
                {
                    msg.pID = currentPlayer.id;
                    msg.missile = FireMissile.Missile1;
                    send = true;
                    //playerData.op = PlayerOp.fireMissile;
                    //playerData.pIndex = 0;
                    //playerData.pID = currentPlayer.id; 
                    //PlayerQueue.add(playerData);
                }

            }

            if (oldState.IsKeyDown(Keys.C) && newState.IsKeyUp(Keys.C) || (P1oldPadState.IsButtonDown(Buttons.B) && P1newPadState.IsButtonUp(Buttons.B)))
            {
                if (currentPlayer.state == PlayerState.alive && BombManager.Instance().bombAvailable(currentPlayer.id))
                {
                    //DDC
                    msg.pID = currentPlayer.id;
                    msg.bomb = DropBomb.Bomb1; 
                    send = true;
                    //gomData.op = GOMOp.createBomb;
                    //gomData.pID = PlayerID.one;
                    //GameObjManager.Instance().doWork(gomData);
                    //GameObjManager.Instance().createBomb(PlayerID.one);
                }
            }

            //netSim stuff
            if (gamer.IsHost)
            {
                if (oldState.IsKeyDown(Keys.K) && newState.IsKeyUp(Keys.K))
                {
                    //netquality
                    if (netPred.netQuality == NetworkQuality.Poor)
                    {
                        netPred.netQuality = NetworkQuality.Good;
                    }
                    else if (netPred.netQuality == NetworkQuality.Good)
                    {
                        netPred.netQuality = NetworkQuality.Perfect;
                    }
                    else
                    {
                        netPred.netQuality = NetworkQuality.Poor;
                    }

                }

                if (oldState.IsKeyDown(Keys.L) && newState.IsKeyUp(Keys.L))
                {
                    //packetsPerFrame
                    if (netPred.framesBetweenPacket == 1)
                    {
                        netPred.framesBetweenPacket = 3;
                    }
                    else if (netPred.framesBetweenPacket == 3)
                    {
                        netPred.framesBetweenPacket = 6;
                    }
                    else
                    {
                        netPred.framesBetweenPacket = 1;
                    }
                }

                if (oldState.IsKeyDown(Keys.O) && newState.IsKeyUp(Keys.O))
                {
                    //smoothing
                    if (netPred.smoothing == OnOff.off)
                    {
                        netPred.smoothing = OnOff.on;
                    }
                    else
                    {
                        netPred.smoothing = OnOff.off;
                    }
                }

                if (oldState.IsKeyDown(Keys.P) && newState.IsKeyUp(Keys.P))
                {
                    //prediction
                    if (netPred.prediction == OnOff.off)
                    {
                        netPred.prediction = OnOff.on;
                    }
                    else
                    {
                        netPred.prediction = OnOff.off;
                    }
                }
            }

            if (send)
            {
                OutputQueue.Instance.add(msg);
                send = false;
            }

            P1oldPadState = P1newPadState;
            P2oldPadState = P2newPadState;
            oldState = newState;
        }

        private void clearData()
        {
            TextureManager.Instance().clear();
            ImageManager.Instance().clear();
            SpriteBatchManager.Instance().clear();
            SpriteProxyManager.Instance().clear();
            DisplayManager.Instance().clear();
            AnimManager.Instance().clear();
            GameObjManager.Instance().clear();
            Timer.Clear();
            PlayerManager.Instance().clear();
            BombManager.Instance().clear();
        }

        public void resetData()
        {
            clearData();

            LoadContent();

            ScoreManager.Instance().createData();

            state = gameState.game;
        }

        public void HookSessionEvents()
        {
            netSession.GamerJoined += GamerJoinedEventHandler;
            netSession.GamerLeft += GamerLeftEventHandler;
            netSession.SessionEnded += SessionEndedEventHandler;
        }

        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = netSession.AllGamers.IndexOf(e.Gamer);
            if (e.Gamer == netSession.Host)
            {
                currentPlayer = players[0];
                gamer = netSession.LocalGamers[0];
            }
            else
            {
                currentPlayer = players[1];
                gamer = netSession.LocalGamers[0];
            }
            e.Gamer.Tag = currentPlayer;
        }

        void GamerLeftEventHandler(object sender, GamerLeftEventArgs e)
        {
            if (netSession == null) 
            {}
            else
            {
                netSession.Dispose();
                netSession = null;
                menu.message = "Press A to create session:\n Press B to join session:";
                menu.created = false;
            }

            OutputQueue.Instance.clear();
            InputQueue.Instance.clear();
            gamer = null;
            GameObject.resetCnt();
            Data.Instance().clear();
            ScoreManager.Instance().clear();
            resetData();
            state = gameState.menu;
            netCount++;
        }

        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            netSession.Dispose();
            netSession = null;
            menu.message = "Press A to create session:\n Press B to join session:";
            menu.created = false;
            OutputQueue.Instance.clear();
            InputQueue.Instance.clear();
            gamer = null;
            GameObject.resetCnt();
            Data.Instance().clear();
            ScoreManager.Instance().clear();
            resetData();
            state = gameState.menu;
        }

        public void updatePlayers()
        {
            if (gamer == netSession.Host)
            {
                currentPlayer = players[0];
            }
            else
            {
                currentPlayer = players[1];
            }
        }
    }
}
