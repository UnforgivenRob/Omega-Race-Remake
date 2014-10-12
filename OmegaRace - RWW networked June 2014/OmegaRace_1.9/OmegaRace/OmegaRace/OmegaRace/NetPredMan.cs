using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OmegaRace;
using SpriteAnimation;
using CollisionManager;

namespace OmegaRace
{
    public enum NetworkQuality
    {
        Poor,
        Good,
        Perfect,
    }

    public enum OnOff
    {
        on,
        off,
    }

    public class NetPredMan
    {
        String s;
        SpriteBatch sb;
        public NetworkQuality netQuality;
        public int framesBetweenPacket;
        public OnOff prediction;
        public OnOff smoothing;
        Vector2 textPos = new Vector2(20, 20);
        Game1 g;

        public NetPredMan(Game1 g)
        {
            this.g = g;
            netQuality = NetworkQuality.Perfect;
            framesBetweenPacket = 1;
            prediction = OnOff.off;
            smoothing = OnOff.off;
            sb = new SpriteBatch(g.GraphicsDevice);
        }

        public void update()
        {
            if (g.netSession == null) return;
            if (g.gamer.IsHost)
            {
                g.netSession.SessionProperties[0] = (int)netQuality;
                g.netSession.SessionProperties[1] = framesBetweenPacket;
                g.netSession.SessionProperties[2] = (int)prediction;
                g.netSession.SessionProperties[3] = (int)smoothing;
            }
            else
            {
                if (g.netSession.SessionProperties[0] == null || g.netSession.SessionProperties[1] == null || g.netSession.SessionProperties[2] == null 
                    || g.netSession.SessionProperties[3] == null) return;
                netQuality = (NetworkQuality)g.netSession.SessionProperties[0];
                framesBetweenPacket = (int)g.netSession.SessionProperties[1];
                prediction = (OnOff)g.netSession.SessionProperties[2];
                smoothing = (OnOff)g.netSession.SessionProperties[3];

                //Check for prediction and smoothing updates on ships
                ShipPredSmoothUpdate(PlayerManager.Instance().getPlayer(PlayerID.one).playerShip);
                ShipPredSmoothUpdate(PlayerManager.Instance().getPlayer(PlayerID.two).playerShip);
            }

            if (netQuality == NetworkQuality.Poor)
            {
                g.netSession.SimulatedLatency = TimeSpan.FromMilliseconds(200);
                g.netSession.SimulatedPacketLoss = .2f;
            }
            else if (netQuality == NetworkQuality.Good)
            {
                g.netSession.SimulatedLatency = TimeSpan.FromMilliseconds(100);
                g.netSession.SimulatedPacketLoss = .1f;
            }
            else
            {
                g.netSession.SimulatedLatency = TimeSpan.Zero;
                g.netSession.SimulatedPacketLoss = 0;
            }
        }

        private void ShipPredSmoothUpdate(Ship s)
        {
            s.UpdatePredSmooth(g);
        }


        public void draw()
        {
            s = "Network Quality(K): " + Enum.GetName(netQuality.GetType(), netQuality) + "\nPacket Speed(L): " + 60/framesBetweenPacket + "\nPrediction(P): " 
                + Enum.GetName(prediction.GetType(), prediction) + "\nSmoothing(O): " + Enum.GetName(smoothing.GetType(), smoothing);
            sb.Begin();

            sb.DrawString(((XNA_Font)TextureManager.Instance().getText(TextEnum.font2).texture).src, s, textPos, Color.Red);
            sb.End();
        }

    }
}
