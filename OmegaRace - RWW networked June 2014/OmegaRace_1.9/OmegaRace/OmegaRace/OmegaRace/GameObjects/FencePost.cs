﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpriteAnimation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace CollisionManager
{
    class FencePost : GameObject
    {
        public FencePost(GameObjType _type, Sprite_Proxy _spriteRef)
            : base(PlayerID.one)
        {
            type = _type;
            spriteRef = _spriteRef;
        }

        public override void Update()
        {
        }
    }
}
