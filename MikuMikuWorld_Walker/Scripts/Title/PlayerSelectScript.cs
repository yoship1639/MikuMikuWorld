using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Input;
using MikuMikuWorld.Controls;
using OpenTK;

namespace MikuMikuWorld.Scripts
{
    class PlayerSelectScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        MenuInputResolver input;

        TransitControl transit;
        Label label;
        TabControl tabCtr;

        float transition = 0.0f;
        bool trans = false;

        InitLoading load;

        GameObject player;

        public ImportedObject SelectedPlayer { get; private set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            MMW.FindGameComponent<BackgroundScript>().Trans(new Color4(222, 216, 148, 255), 0.25);

            SelectedPlayer = null;

            input = new MenuInputResolver();
            input.Up = Key.W;
            input.Down = Key.S;
            input.Right = Key.D;
            input.Left = Key.A;

            transit = new TransitControl();
            transit.LocalLocation = new Vector2(MMW.ClientSize.Width * 2.0f, 0);
            transit.Size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            transit.Target = Vector2.Zero;

            load = MMW.FindGameComponent<InitLoading>();

            tabCtr = new TabControl()
            {
                Parent = transit,
                LocalLocation = new Vector2(100, 164),
                Size = new Vector2((MMW.ClientSize.Width / 2) - 100 - 64, MMW.ClientSize.Height - 164 - 48),
                Tabs = new Tab[]
                {
                    new Tab() { Name = "PRESET", Items = load.PresetCharacters, },
                    new Tab() { Name = "FREE", Items = load.FreeCharacters, },
                },
                Focus = true,
            };

            label = new Label()
            {
                Parent = transit,
                Alignment = ContentAlignment.TopCenter,
                Text = "PLAYER SELECT",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };

            MMW.MainCamera.GameObject.Transform.Position = new Vector3(0.6f, 1.1f, -1.7f);
            //MMW.MainCamera.GameObject.Transform.Position = new Vector3(0.3f, 1.4f, -0.6f);
            MMW.MainCamera.ShadowMapping = false;

            MMW.GlobalAmbient = new Color4(0.8f, 0.8f, 0.8f, 0.0f);
            //MMW.DirectionalLight.Transform.Rotate.Y = (float)Math.PI;
            MMW.DirectionalLight.Transform.Rotate = new Vector3(-MathHelper.PiOver3, 0.0f, 0.0f);
            MMW.DirectionalLight.Intensity = 1.0f;
            MMW.IBLIntensity = 0.0f;

            
            if (load.State != InitLoading.LoadingState.Finished)
            {
                load.LoadCompleted += Load_LoadCompleted;
            }
        }

        private void Load_LoadCompleted(object sender, LoadingEventArgs e)
        {
            if (e.State == InitLoading.LoadingState.PresetCharacter)
            {
                tabCtr.Tabs[0].Items = load.PresetCharacters;
            }
            else if (e.State == InitLoading.LoadingState.FreeCharacter)
            {
                tabCtr.Tabs[1].Items = load.FreeCharacters;
            }
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            transition += (trans ? -1.0f : 1.0f) * (float)deltaTime * 5.0f;
            transition = MMWMath.Saturate(transition);

            transit.Update(deltaTime);

            if (AcceptInput && !trans)
            {
                input.Update(deltaTime);

                var prev = tabCtr.SelectedObject;

                if (input.IsRight)
                {
                    tabCtr.NextTab();
                    if (prev != tabCtr.SelectedObject)
                    {
                        MMW.DestroyGameObject(player);
                        player = null;
                    }
                }
                else if (input.IsLeft)
                {
                    tabCtr.PrevTab();
                    if (prev != tabCtr.SelectedObject)
                    {
                        MMW.DestroyGameObject(player);
                        player = null;
                    }
                }
                else if (input.IsDown)
                {
                    tabCtr.NextSelect();
                    if (prev != tabCtr.SelectedObject)
                    {
                        MMW.DestroyGameObject(player);
                        player = null;
                    }
                }
                else if (input.IsUp)
                {
                    tabCtr.PrevSelect();
                    if (prev != tabCtr.SelectedObject)
                    {
                        MMW.DestroyGameObject(player);
                        player = null;
                    }
                }
                else if (input.IsBack)
                {
                    if (player != null) MMW.DestroyGameObject(player);
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    GameObject.AddComponent<TitleScript>();
                    
                }
                else if (input.IsSelect && player != null)
                {
                    MMW.DestroyGameObject(player);
                    SelectedPlayer = (ImportedObject)tabCtr.SelectedObject;
                    MMW.GetAsset<GameData>().Player = SelectedPlayer;
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    GameObject.AddComponent<StageSelectScript>();
                }
            }

            if (trans && transition < 0.01f) Destroy();

            if (player == null && tabCtr.SelectedObject != null)
            {
                var path = ((ImportedObject)tabCtr.SelectedObject).Path;

                var go = GameObjectFactory.CreateMeshObject(path, MMW.GetAsset<GameData>().SkinShader);
                //go.Layer = 1;
                go.Transform.Rotate.Y = -2.8f;
                go.UpdateAction += (s, e) =>
                {
                    //go.Transform.Rotate.Y += (float)e.deltaTime * 0.5f;
                };

                // Test: モーションを取得
                var motions = Array.FindAll(load.PresetObjects, o => o.Type == ImportedObjectType.Motion && o.Property != null);
                if (motions.Length > 0)
                {
                    var animator = go.AddComponent<ComputeAnimator>();
                    var mr = go.GetComponent<MeshRenderer>();
                    animator.Bones = mr.Bones;
                    foreach (var m in motions)
                    {
                        var impo = MMW.GetSupportedImporter(m.Path);
                        var mo = impo.Import(m.Path, Importers.ImportType.Full)[0];
                        animator.AddMotion(mo.Name, mo.Motions[0]);
                    }
                    //animator.SetRate("secret", 1.0f);
                    animator.SetRate("nekomimi_mikuv2", 1.0f);
                    animator.Frame = 0.0f;
                    go.AddComponent<AnimationController>();
                    //go.UpdateAction += (se, e) => 
                    //{
                        //animator.Frame = MMWMath.Repeat(animator.Frame + ((float)e.deltaTime * 30.0f), 0.0f, 5000.0f);
                        //if (Input.IsKeyDown(Key.Right)) animator.AddRate("nekomimi_mikuv2", (float)e.deltaTime, 0.0f, 1.0f);
                        //if (Input.IsKeyDown(Key.Left)) animator.AddRate("nekomimi_mikuv2", -(float)e.deltaTime, 0.0f, 1.0f);
                        //if (Input.IsKeyDown(Key.Right)) go.Transform.Rotate.Y += (float)e.deltaTime;
                        //if (Input.IsKeyDown(Key.Left)) go.Transform.Rotate.Y -= (float)e.deltaTime;
                    //};
                }

                MMW.RegistGameObject(go);
                player = go;
            }
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();
            //var g = Drawer.BindGraphicsDirect();

            label.Draw(g, deltaTime);
            tabCtr.Draw(g, deltaTime);

            Drawer.IsGraphicsUsed = true;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            load.LoadCompleted -= Load_LoadCompleted;
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
