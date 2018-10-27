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
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.Lights;
using System.IO;
using System.Diagnostics;
using MikuMikuWorld.GameComponents.ImageEffects;
using OpenTK.Audio.OpenAL;

namespace MikuMikuWorld.Scripts
{
    class StageSelectScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        MenuInputResolver input;

        TransitControl transit;
        Label label;
        TabControl tabCtr;

        float transition = 0.0f;
        bool trans = false;

        InitLoading load;

        public ImportedObject SelectedStage { get; private set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            SelectedStage = null;

            input = new MenuInputResolver();
            input.Up = Key.W;
            input.Down = Key.S;
            input.Right = Key.D;
            input.Left = Key.A;

            load = MMW.FindGameComponent<InitLoading>();
            if (load.State != InitLoading.LoadingState.Finished)
            {
                load.LoadCompleted += Load_LoadCompleted;
            }

            transit = new TransitControl();
            transit.LocalLocation = new Vector2(MMW.ClientSize.Width * 2.0f, 0);
            transit.Size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            transit.Target = Vector2.Zero;

            tabCtr = new TabControl()
            {
                Parent = transit,
                LocalLocation = new Vector2(100, 164),
                Size = new Vector2((MMW.ClientSize.Width / 2) - 100 - 64, MMW.ClientSize.Height - 164 - 48),
                Tabs = new Tab[]
                {
                    new Tab() { Name = "PRESET", Items = load.PresetStages, },
                    new Tab() { Name = "FREE", Items = load.FreeStages, },
                },
                Focus = true,
            };

            label = new Label()
            {
                Parent = transit,
                Alignment = ContentAlignment.TopCenter,
                Text = "STAGE SELECT",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };
        }

        private void Load_LoadCompleted(object sender, LoadingEventArgs e)
        {
            if (e.State == InitLoading.LoadingState.PresetStage)
            {
                tabCtr.Tabs[0].Items = load.PresetStages;
            }
            else if (e.State == InitLoading.LoadingState.FreeStage)
            {
                tabCtr.Tabs[1].Items = load.FreeStages;
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

                if (input.IsRight)
                {
                    tabCtr.NextTab();
                }
                else if (input.IsLeft)
                {
                    tabCtr.PrevTab();
                }
                else if (input.IsDown)
                {
                    tabCtr.NextSelect();
                }
                else if (input.IsUp)
                {
                    tabCtr.PrevSelect();
                }
                else if (input.IsBack)
                {
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    GameObject.AddComponent<PlayerSelectScript>();
                }
                else if (input.IsSelect && tabCtr.SelectedObject != null)
                {
                    SelectedStage = (ImportedObject)tabCtr.SelectedObject;
                    MMW.GetAsset<GameData>().Stage = SelectedStage;
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    var loadScript = MMW.FindGameComponent<LoadingScript>();
                    loadScript.StartLoading(null, (args) =>
                    {
                        var data = MMW.GetAsset<GameData>();

                        // プレイヤ
                        var player = GameObjectFactory.CreateMeshObject(data.Player.Path, data.SkinShader);
                        //player.Layer = 20;
                        player.AddComponent<SetNameScript>();
                        player.Tags.Add("player");
                        
                        var motions = Array.FindAll(load.PresetObjects, o => o.Type == ImportedObjectType.Motion && o.Property != null);
                        if (motions.Length > 0)
                        {
                            var animator = player.AddComponent<ComputeAnimator>();
                            var mr = player.GetComponent<MeshRenderer>();
                            animator.Bones = mr.Bones;
                            foreach (var m in motions)
                            {
                                var impo = MMW.GetSupportedImporter(m.Path);
                                var mo = impo.Import(m.Path, Importers.ImportType.Full)[0];
                                animator.AddMotion(mo.Name, mo.Motions[0]);
                            }
                            animator.SetRate("nekomimi_mikuv2", 1.0f);
                            player.AddComponent<AnimationController>();
                        }

                        // テストライト
                        {
                            var pl = player.AddComponent<PointLight>();
                            pl.Intensity = 1.0f;
                            pl.Radius = 4.0f;
                            pl.Color = Color4.White;
                        }

                        {
                            var c = player.AddComponent<CapsuleCollider>(0.3f, 1.0f);
                            c.Position.Y = 0.8f;
                            var rb = player.AddComponent<RigidBody>();
                            rb.Mass = 50.0f;
                            rb.FreezeRotation = true;
                            rb.DisableDeactivation = true;
                            rb.LinearDamping = 0.3f;
                            rb.LinearVelocityLimitXZ = 2.0f;
                            rb.Friction = 0.95f;
                            player.UpdateAction += (s, ev) =>
                            {
                                player.Transform.Rotate.Y += Input.MouseDelta.X * 0.0025f;
                                MMW.MainCamera.Transform.Rotate.X += Input.MouseDelta.Y * 0.0025f;
                                MMW.MainCamera.Transform.Rotate.X = MathHelper.Clamp(MMW.MainCamera.Transform.Rotate.X, -1.0f, 1.0f);

                                var front = player.Transform.WorldDirectionZ;
                                var left = player.Transform.WorldDirectionX;
                                var speed = (float)ev.deltaTime * 2.0f;

                                var deltaDir = Vector3.Zero;
                                if (Input.IsKeyDown(Key.W)) deltaDir += front;
                                if (Input.IsKeyDown(Key.S)) deltaDir -= front;
                                if (Input.IsKeyDown(Key.A)) deltaDir += left;
                                if (Input.IsKeyDown(Key.D)) deltaDir -= left;

                                if (deltaDir != Vector3.Zero)
                                {
                                    deltaDir.Normalize();
                                    //rb.ApplyImpulse(deltaDir * speed * 80.0f);
                                    player.Transform.Position += deltaDir * speed;
                                }
                                if (Input.IsKeyPressed(Key.Space)) rb.ApplyImpulse(Vector3.UnitY * 36.0f);

                                player.Transform.UpdatePhysicalTransform();

                                var cam = MMW.MainCamera;
                                var pos = cam.Transform.WorldPosition;
                                //var vel = (cam.Transform.Position - prevPos) * (float)deltaTime;
                                var dirv = cam.Transform.WorldDirectionZ;
                                var ori = new float[] { dirv.X, dirv.Y, dirv.Z, cam.Up.X, cam.Up.Y, cam.Up.Z };
                                AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, ref pos);
                                //AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Velocity, ref vel);
                                AL.Listener(OpenTK.Audio.OpenAL.ALListenerfv.Orientation, ref ori);
                            };

                            MMW.MainCamera.ShadowMapping = true;
                            MMW.MainCamera.Transform.Parent = player.Transform;
                            MMW.MainCamera.GameObject.UpdateAction += (s, ev) =>
                            {
                                if (Input.IsKeyDown(Key.ShiftLeft))
                                {
                                    MMW.MainCamera.Transform.Position = new Vector3(-0.5f, 1.5f, -0.5f);
                                }
                                else
                                {
                                    MMW.MainCamera.Transform.Position = new Vector3(0.0f, 1.5f, -1.3f);
                                }

                                //MMW.DirectionalLight.Transform.Rotate.Y += (float)ev.deltaTime * 0.01f;
                            };
                        }

                        // ステージ
                        var stage = GameObjectFactory.CreateStage(data.Stage.Path, data.Shader);
                        stage.Tags.Add("stage");
                        {
                            var sCol = stage.GetComponent<PhysicalGameComponent>();
                            sCol.Collide += SCol_Collide;
                        }

                        MMW.RegistGameObject(player);
                        MMW.RegistGameObject(stage);

                        for (var i = 0; i < 1; i++)
                        {
                            var tsts = GameObjectFactory.CreateMeshObject(data.Player.Path, data.SkinShader);
                            tsts.Tags.Add("test");
                            //tsts.Layer = 20;
                            tsts.AddComponent<SetNameScript>();
                            tsts.Transform.Position.Z = (i + 1) * 1.0f;
                            var c = tsts.AddComponent<CapsuleCollider>(0.3f, 1.0f);
                            c.Position.Y = 0.8f;
                            var rb = tsts.AddComponent<RigidBody>();
                            rb.Mass = 50.0f;
                            rb.FreezeRotation = true;
                            rb.DisableDeactivation = true;
                            rb.LinearDamping = 0.3f;

                            var animator = tsts.AddComponent<ComputeAnimator>();
                            var mr = tsts.GetComponent<MeshRenderer>();
                            animator.Bones = mr.Bones;
                            foreach (var m in motions)
                            {
                                var impo = MMW.GetSupportedImporter(m.Path);
                                var mo = impo.Import(m.Path, Importers.ImportType.Full)[0];
                                animator.AddMotion(mo.Name, mo.Motions[0]);
                            }
                            animator.SetRate("nekomimi_mikuv2", 1.0f);
                            var s = i + 1;
                            var ac = tsts.AddComponent<AnimationController>();
                            ac.Speed = i * 0.25;

                            var sound = new Sound("C:/Users/yoshihiro/Downloads/dbg2.wav");
                            sound.Load();
                            var sc = tsts.AddComponent<SoundController>();
                            sc.Sounds.Add("test", sound);
                            sc.Play("test", 1.0f);

                            tsts.UpdateAction += (se, e) =>
                            {
                                tsts.Transform.Position = new Vector3((float)Math.Sin(MMW.TotalElapsedTime * 4.0) * 4.0f, 0.0f, (float)Math.Cos(MMW.TotalElapsedTime * 4.0) * 4.0f);
                            };

                            MMW.RegistGameObject(tsts);
                        }

                        var effs = MMW.MainCamera.GameObject.GetComponents<ImageEffect>();
                        foreach (var eff in effs) eff.Enabled = true;

                        MMW.GlobalAmbient = new Color4(0.2f, 0.18f, 0.16f, 0.0f);
                        MMW.DirectionalLight.Intensity = 6.0f;
                        MMW.IBLIntensity = 0.0f;

                        return true;
                    });
                    loadScript.LoadCompleted += Load_LoadCompleted;
                }
            }

            if (trans && transition < 0.01f) Destroy();
        }

        private void SCol_Collide(object sender, Collision e)
        {
            var gc = ((PhysicalGameComponent)sender);
            var v = e.TotalExtrusion;

            var n = v.Normalized();
            if (Math.Abs(n.Y) > 0.5f) return;
            v.Y = 0.0f;

            if (e.GameObject.Tags.Contains("player"))
            {
                var rb = e.GameObject.GetComponent<RigidBody>();
                rb.ApplyImpulse(v * 10.0f);
            }
        }

        private void Load_LoadCompleted(object sender, object e)
        {
            MMW.DestroyGameObjects(g => g.Name == "Background" || g.Name == "Title");
            var load = MMW.FindGameComponent<LoadingScript>();
            load.LoadCompleted -= Load_LoadCompleted;
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();

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
