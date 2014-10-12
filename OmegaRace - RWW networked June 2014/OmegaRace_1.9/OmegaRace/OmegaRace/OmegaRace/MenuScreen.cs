using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using SpriteAnimation;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace OmegaRace
{
    class MenuScreen
    {
        private KeyboardState newState;
        private KeyboardState oldState;
        private GamePadState P1newPadState;
        private GamePadState P2newPadState;
        private GamePadState P1oldPadState;
        private GamePadState P2oldPadState;
        private SpriteBatch sb;
        public string message;
        private Game1 g;
        public bool created;

        private Vector2 textPos = Vector2.Zero;
        private Vector2 menuPos = new Vector2(0, 100);
        private Rectangle boxPos = new Rectangle(350, 370, 550, 110);

        public MenuScreen(Game1 g)
        {
            this.g = g;
            this.created = false;
            oldState = Keyboard.GetState();
            P1oldPadState = GamePad.GetState(PlayerIndex.One);
            P2oldPadState = GamePad.GetState(PlayerIndex.Two);
            sb = new SpriteBatch(g.GraphicsDevice);
        }

        public void update()
        {
            newState = Keyboard.GetState();
            P1newPadState = GamePad.GetState(PlayerIndex.One);
            P2newPadState = GamePad.GetState(PlayerIndex.Two);

            if (g.IsActive)
            {
                if (created)
                {
                    if (oldState.IsKeyDown(Keys.B) && newState.IsKeyUp(Keys.B) ||
                    P1oldPadState.IsButtonDown(Buttons.B) && P1newPadState.IsButtonUp(Buttons.B))
                    {
                        if (g.netSession == null)
                        {
                        }
                        else
                        {
                            g.netSession.Dispose();
                            g.netSession = null;
                        }
                        message = "Press A to create session:\n Press B to join session:";
                        created = false;
                    }

                    if (g.netSession != null && g.netSession.AllGamers.Count == g.maxGamers)
                    {
                        g.updatePlayers();
                        g.state = gameState.game;
                    }
                }
                else
                {
                    if (Gamer.SignedInGamers.Count == 0)
                    {
                        // If there are no profiles signed in, we cannot proceed.
                        // Show the Guide so the user can sign in.
                        Guide.ShowSignIn(g.maxLocalGamers, false);
                    }
                    else if (oldState.IsKeyDown(Keys.A) && newState.IsKeyUp(Keys.A) ||
                        P1oldPadState.IsButtonDown(Buttons.A) && P1newPadState.IsButtonUp(Buttons.A))
                    {
                        // Create a new session?
                        CreateSession();
                    }
                    else if (oldState.IsKeyDown(Keys.B) && newState.IsKeyUp(Keys.B) ||
                        P1oldPadState.IsButtonDown(Buttons.B) && P1newPadState.IsButtonUp(Buttons.B))
                    {
                        // Join an existing session?
                        JoinSession();
                    }

                    if (g.netSession != null && g.netSession.AllGamers.Count == g.maxGamers)
                    {
                        g.updatePlayers();
                        g.state = gameState.game;
                    }
                }
            }

            if (oldState.IsKeyDown(Keys.Q) || P2oldPadState.IsButtonDown(Buttons.DPadUp))
            {
                g.state = gameState.game;
            }

            P1oldPadState = P1newPadState;
            P2oldPadState = P2newPadState;
            oldState = newState;
        }

        public void draw()
        {
            sb.Begin();

            if (message == null)
            {
                message = "Press A to create session:\n Press B to join session:";
            }

            sb.Draw(((XNA_Text2D)(TextureManager.Instance().getText(TextEnum.Menu).texture)).src, menuPos, Color.White);
            sb.Draw(((XNA_Text2D)(TextureManager.Instance().getText(TextEnum.circle).texture)).src, boxPos, Color.Black);
            sb.DrawString(((XNA_Font)TextureManager.Instance().getText(TextEnum.font).texture).src, message, textPos, Color.White);
            sb.End();
        }

        private void CreateSession()
        {
            OutputQueue.Instance.clear();
            InputQueue.Instance.clear();
            //DrawMessage("Creating session...");
            if (g.netSession == null)
            {
                g.netSession = NetworkSession.Create(NetworkSessionType.SystemLink, g.maxLocalGamers, g.maxGamers);
            }
            g.HookSessionEvents();
            created = true;
            message = "Waiting for another player...\n Press B to return to menu ";
            //g.state = gameState.game;
        }

        void JoinSession()
        {
            OutputQueue.Instance.clear();
            InputQueue.Instance.clear();
            // Search for sessions.
            using (AvailableNetworkSessionCollection availableSessions =
                  NetworkSession.Find(NetworkSessionType.SystemLink, g.maxLocalGamers, null))
            {
                if (availableSessions.Count == 0)
                {
                    message = "No network sessions found.";
                    Timer.Add(new TimeSpan(0, 0, 5) + Timer.GetCurrentTime(), new object(), JoinSessionCallback);
                    return;
                }

                // Join the first session we found.
                message = "Joining Session...";
                g.netSession = NetworkSession.Join(availableSessions[0]);

                g.HookSessionEvents();
            } 
        }

        public void JoinSessionCallback(object o)
        {
            this.message = "Press A to create session:\n Press B to join session:";
            return;
        }
    }
}