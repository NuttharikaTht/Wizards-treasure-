﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Projectile
{
    public class World
    {
        public Thief thief;
        public Thief thiefNew;
        public Wizard wizard;
        public Arrow arrow;

        public static Floor floor;
        public static Treasure treasure;

        public Slots slot;
        public Vector2 offset;
        public List<obj> projectiles = new List<obj>();

        public Rectangle descriptionBox;
        public Texture2D boxTexture;
        public Rectangle itemShowBox;
        public Texture2D itemTexture;
        public string description;

        public SpriteFont engFonts, thaiFont, itemNameFont, descriptionFont;

        public String nameI;
        public World()
        {

            engFonts = Globals.content.Load<SpriteFont>("fonts/Minecraft_24");
            itemNameFont = Globals.content.Load<SpriteFont>("fonts/Minecraft_16");
            descriptionFont = Globals.content.Load<SpriteFont>("fonts/Minecraft_12");

            boxTexture = Globals.content.Load<Texture2D>("textures/descriptionBox");

            floor = new Floor("floor", new Vector2(0, 572), new Vector2(Globals.screenWidth, 88));
            // init wall
            for (int i = 0; i < Globals.slots.Length; i++)
            {
                Globals.slots[i] = new Slots(new Vector2(80 + (40 * i), 572), i);
                {
                    Globals.slots[i].CurrentState = SlotsState.Walk;
                };

            }
           

            Globals.CurrentPlayer = WhoPlay.Wizard;
            thief = new Thief("player/thief", new Vector2(40, 542), new Vector2(90, 90), 500);
            wizard = new Wizard("player/wizard", new Vector2(1180, 542), new Vector2(90, 90));

            treasure = new Treasure("treasure", new Vector2(1100, 522), new Vector2(71, 50));

            //set PaddProjectile to AddProjectile Func
            GameGlobals.PassProjectile = AddProjectile;

        }

        public virtual void Update(GameTime gameTime)
        {
            if (Globals.CurrentStatus == WhoWin.Non)
            {
                if (Globals.CurrentPlayer == WhoPlay.Thief)
                {
                    thief.Update(gameTime);
                    if (thief.checkAim())
                    {
                        thief.arrow.Update(gameTime);
                        nameI = thief.arrow.itemName;
                        ObjectDescription(nameI);
                    }
                }
                else
                {
                    wizard.Update(gameTime);
                    if (wizard.checkAim())
                    {
                        wizard.arrow.Update(gameTime);
                        nameI = wizard.arrow.itemName;
                        ObjectDescription(nameI);
                    }
                }

                if (projectiles.Count > 0)
                {
                    projectiles[0].Update(gameTime, offset, null);
                    if (projectiles[0].isDone)
                    {
                        // x = slot that projectile hit
                        float x = (projectiles[0].pos.X - 80) / 40;
                        int index = (int)MathF.Round(x, MidpointRounding.AwayFromZero);

                        if (Globals.CurrentPlayer == WhoPlay.Wizard && index > 0)
                        {
                            // add wall in slot index x
                            AddWall(index, projectiles[0].type, gameTime);
                        }

                        if (Globals.CurrentPlayer == WhoPlay.Thief && index < 26)
                        {
                            ItemEffect(index);
                        }

                        projectiles.RemoveAt(0);
                        WordSwapPalyer();
                    }
                }
            }
            if (Globals.timer <= 0)
            {
                WordSwapPalyer();
            }
        }
        public void WordSwapPalyer() {
            thief.CurrentState = PlayerState.Idle;
            wizard.CurrentState = PlayerState.Idle;
            Globals.SwapPlayer();
            switch (Globals.CurrentPlayer)
            {
                case WhoPlay.Thief: thief.CurrentState = PlayerState.Running; break;
                case WhoPlay.Wizard: wizard.CurrentState = PlayerState.Running; break;
            }
        }
        public void AddWall(int INDEX, String type, GameTime gameTime)
        {
            WallType wall = WallType.Non;
            switch (type)
            {
                case "dirt": wall = WallType.Dirt; break;
                case "water": wall = WallType.Water; break;
                case "wind": wall = WallType.Wind; break;
                case "fire": wall = WallType.Fire; break;
            }
            Globals.slots[INDEX].Update(gameTime, wall);
        }

        public void ItemEffect(int INDEX)
        {
            if (nameI == "BlackHole")
            {
                //move thief
                Globals.slots[INDEX].DestroyWall();
                thief.pos = new Vector2(projectiles[0].pos.X, thief.pos.Y);
                thief.staminaRect.X = (int)thief.pos.X - 40;
                thief.staminaRect.Y = (int)thief.pos.Y - 80;
            }
            else if (nameI == "Letter")
            {
                //win
                Random R = new Random();
                if (R.Next(1, 100) == 1)
                {
                    Globals.CurrentStatus = WhoWin.Thief;
                }
            }
            else if (nameI == "Banana")
            {
                //เพิ่ม mp
                thief.staminaUp();
            }
            if (thief.arrow.collision(thief.arrow.item.rect, Globals.slots[INDEX].rect))
            {

                if (nameI == "Titan")
                {
                    //ทำลายกำแพง 1 ตึก
                    Globals.slots[INDEX].DestroyWall();
                }

                else if (nameI == "Jerry")
                {
                    //ทำลายกำแพงเหลือ 1
                    Globals.slots[INDEX].Drop();
                }
                else if (nameI == "Missile")
                {
                    //ทำลายกำแพง 1
                    Globals.slots[INDEX].DownLevel();
                }
                else if (nameI == "Water")
                {
                    //nothing
                }
                else if (nameI == "Flower")
                {
                    //nothing
                }

            }
        }

        public void ObjectDescription(string objname)
        {
            descriptionBox = new Rectangle(50, 50, 450, 100);
            itemShowBox = new Rectangle(70, 80, 40, 40);
            if (Globals.CurrentPlayer == WhoPlay.Thief)
            {
                itemTexture = Globals.content.Load<Texture2D>("textures/ammo/thief/" + objname);
                if (objname.Equals("Titan")) description = "Destroy the whole wall";
                else if (objname.Equals("Water")) description = "Fresh Drink... For Drink (No Effect)";
                else if (objname.Equals("Missile")) description = "Down Wall 1 Level";
                else if (objname.Equals("Letter")) description = "Love Letter, Try Use this!\nYou have 1% Chance to win Wizard's Heart";
                else if (objname.Equals("Jerry")) description = "Down Wall to Level 1";
                else if (objname.Equals("Flower")) description = "Just...Stupid Flower";
                else if (objname.Equals("BlackHole")) description = "Warp to Target Position via Black Hole";
                else if (objname.Equals("Banana")) description = "Reject Humanity Return to Monkey" + "\n" + "(Stamina +30%)";
            }

            if (Globals.CurrentPlayer == WhoPlay.Wizard)
            {
                itemTexture = Globals.content.Load<Texture2D>("textures/ammo/wizard/" + objname);
                description = "Match with same Element to Upgrade the Wall";
            }
        }
        public virtual void AddProjectile(object INFO)
        {
            projectiles.Add((obj)INFO);
        }

        public virtual void Draw(Vector2 OFFSET)
        {
            floor.Draw();
            treasure.Draw();
            wizard.Draw(OFFSET);
            thief.Draw(OFFSET);
            foreach (Slots slot in Globals.slots)
            {
                slot.Draw(OFFSET);
            }
            if (Globals.CurrentPlayer == WhoPlay.Thief)
            {
                Globals.spriteBatch.DrawString(engFonts, "Thief's Turn", new Vector2(900, 45), Color.Yellow);
            }
            else
            {
                Globals.spriteBatch.DrawString(engFonts, "Wizard's Turn", new Vector2(900, 45), Color.Yellow);
            }

            if (thief.checkAim())
            {
                thief.arrow.Draw(OFFSET);
                Globals.spriteBatch.Draw(boxTexture, descriptionBox, Color.White);
                Globals.spriteBatch.Draw(itemTexture, itemShowBox, Color.White);
                Globals.spriteBatch.DrawString(itemNameFont, nameI, new Vector2(130, 80), Color.Yellow);
                Globals.spriteBatch.DrawString(descriptionFont, description, new Vector2(130, 105), Color.White);
            }

            if (wizard.checkAim())
            {
                wizard.arrow.Draw(OFFSET);
                Globals.spriteBatch.Draw(boxTexture, descriptionBox, Color.White);
                Globals.spriteBatch.Draw(itemTexture, itemShowBox, Color.White);
                Globals.spriteBatch.DrawString(itemNameFont, nameI, new Vector2(130, 80), Color.Yellow);
                Globals.spriteBatch.DrawString(descriptionFont, description, new Vector2(130, 105), Color.White);
            }
            //projectile.Draw(OFFSET);
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(offset);
            }
        }
    }
}
