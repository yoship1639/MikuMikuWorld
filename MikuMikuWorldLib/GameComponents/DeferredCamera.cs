using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    class DeferredCamera : Camera
    {
        public float ForceRenderDistance { get; set; } = 5.0f;

        protected RenderTexture deferredRT;
        protected Shader deferredShader;

        protected DeferredBackgroundShader backShader;
        protected Vector3 prevCameraDir;
        protected Mesh cubeMesh;

        protected Mesh boundsMesh;

        protected Matrix4 orthoMat = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

        protected RenderTexture lightRT;
        Mesh plMesh;
        LightCullingShader lightCullingShader;
        int ssbo_count;
        int ssbo_index;
        int[] initCounts;
        int[] initIndices;

        struct BufferLight
        {
            public Vector4 pos;
            public Vector4 color;
            public Vector4 dir;
            public float intensity;
            public float radius;
            public float specCoeff;
            public float innerDot;
            public float outerDot;
            public Vector3 temp;
        }
        int ssbo_light;
        private int[] queries;

        public DeferredCamera() : base () { }
        public DeferredCamera(RenderTexture rt) : base()
        {
            targetTexture = rt;
        }

        protected internal override void OnLoad()
        {
            CreateShadowMap();

            depthShader = (DepthShader)MMW.GetAsset<Shader>("Depth");

            deferredRT = new RenderTexture(MMW.RenderResolution);
            deferredRT.MultiBuffer = 8;
            deferredRT.ColorFormat0 = PixelInternalFormat.Rgba8;            // albedo
            deferredRT.ColorFormat1 = PixelInternalFormat.Rgb16f;           // world pos
            deferredRT.ColorFormat2 = PixelInternalFormat.Rgb16f;           // world normal
            deferredRT.ColorFormat3 = PixelInternalFormat.Rg8;              // physical params
            deferredRT.ColorFormat4 = PixelInternalFormat.Rgb8;             // f0
            deferredRT.ColorFormat5 = PixelInternalFormat.R32f;             // depth
            deferredRT.ColorFormat6 = PixelInternalFormat.R8;               // shadow
            deferredRT.ColorFormat7 = PixelInternalFormat.Rgba16f;          // velocity
            //deferredRT.ColorFormat8 = PixelInternalFormat.Rgb8;           // unique color
            deferredRT.Load();

            deferredShader = new DeferredPhysicalLightingShader();
            deferredShader.Load();

            backShader = new DeferredBackgroundShader();
            backShader.Load();

            cubeMesh = Mesh.CreateSimpleBoxMesh(Vector3.One * 50.0f);
            for (var i = 0; i < cubeMesh.Vertices.Length; i++)
            {
                cubeMesh.Vertices[i].X *= -1.0f;
            }
            cubeMesh.Load();

            // 影響を及ぼすライト
            lightRT = new RenderTexture(MMW.RenderResolution.Width / 16 + 1, MMW.RenderResolution.Height / 16 + 1);
            lightRT.ColorFormat0 = PixelInternalFormat.Rgb8;
            lightRT.Load();
            plMesh = Mesh.CreateSimpleSphereMesh(1.0f);
            lightCullingShader = new LightCullingShader();
            lightCullingShader.Load();
            // ライト数格納
            initCounts = new int[lightRT.Size.Width * lightRT.Size.Height];
            GL.GenBuffers(1, out ssbo_count);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_count);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, initCounts.Length * 4, initCounts, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            // ライトインデックス格納
            initIndices = new int[lightRT.Size.Width * lightRT.Size.Height * 64];
            GL.GenBuffers(1, out ssbo_index);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_index);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, initIndices.Length * 4, initIndices, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            // ライト情報
            {
                BufferLight[] tmps = new BufferLight[2048];
                GL.GenBuffers(1, out ssbo_light);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_light);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, tmps.Length * 20 * 4, tmps, BufferUsageHint.StreamDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");

            randColors = new Color4[ushort.MaxValue + 1];
            for (var i = 0; i < randColors.Length; i++)
            {
                randColors[i] = new Color4(
                    RandomHelper.NextFloat() * 0.5f + 0.5f,
                    RandomHelper.NextFloat() * 0.5f + 0.5f,
                    RandomHelper.NextFloat() * 0.5f + 0.5f,
                    1.0f);
            }

            shadowDepthBias = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

            sp = new ShaderUniqueParameter()
            {
                camera = this,
            };

            // オクルージョンカリング用クエリ
            queries = new int[4096];
            GL.GenQueries(queries.Length, queries);
            colorShader = (ColorShader)MMW.GetAsset<Shader>("Color");

            boundsMesh = Mesh.CreateSimpleBoxMesh(Vector3.One);
        }

        protected internal override void Draw(double deltaTime)
        {
            var objects = MMW.FindGameObjects((obj) => obj.Enabled && !obj.Destroyed /*&& obj.Layer < GameObject.LayerUI && layerMask.Get(obj.Layer)*/);
            var drawMeshDic = new Dictionary<Shader, Dictionary<Material, List<SubMeshData>>>();

            var animList = new List<AAnimator>();
            var morphList = new List<AMorpher>();

            var wp = Transform.WorldPosition;

            var distDic = new Dictionary<GameObject, float>();
            var distTask = Task.Factory.StartNew(() =>
            {
                foreach (var obj in objects)
                {
                    var dist = (wp - obj.Transform.WorldPosition).Length;
                    distDic.Add(obj, dist);
                }
            });

            // オブジェクトレイヤ抽出
            //DrawableGameComponent[] meshComs = null;
            DrawableGameComponent[] beforeComs = null;
            DrawableGameComponent[] afterComs = null;
            //DrawableGameComponent[] uiMeshComs = null;
            DrawableGameComponent[] uiComs = null;
            var layerSortTask = Task.Factory.StartNew(() =>
            {
                var draws = MMW.FindGameComponents<DrawableGameComponent>(c => c.Enabled && !c.Destroyed && !(c is MeshRenderer));

                //var meshList = new List<DrawableGameComponent>();
                var beforeList = new List<DrawableGameComponent>();
                var afterList = new List<DrawableGameComponent>();
                var uiList = new List<DrawableGameComponent>();
                //var uiMeshList = new List<DrawableGameComponent>();

                foreach (var d in draws)
                {
                    if (d.Layer < LayerAfterMeshRender) beforeList.Add(d);
                    else if (d.Layer < LayerUI) afterList.Add(d);
                    else uiList.Add(d);

                    //if (d.Layer < LayerUI) meshList.Add(d);
                    //else uiMeshList.Add(d);
                }

                beforeList.Sort((a, b) => a.Layer - b.Layer);
                afterList.Sort((a, b) => a.Layer - b.Layer);
                uiList.Sort((a, b) => a.Layer - b.Layer);
                //meshList.Sort((a, b) => a.Layer - b.Layer);
                //uiMeshList.Sort((a, b) => a.Layer - b.Layer);

                //var meshObjs = objects.ToArray();
                //var beforeObjs = Array.FindAll(objects, (o) => o.Layer >= 0 && o.Layer < GameObject.LayerAfterRender);
                //var afterObjs = Array.FindAll(objects, (o) => o.Layer >= GameObject.LayerAfterRender);
                //var uiObjs = MMW.FindGameObjects((obj) => obj.Layer >= GameObject.LayerUI && layerMask.Get(obj.Layer));

                //Array.Sort(meshObjs, (a, b) => a.Layer - b.Layer);
                //Array.Sort(beforeObjs, (a, b) => a.Layer - b.Layer);
                //Array.Sort(afterObjs, (a, b) => a.Layer - b.Layer);
                //Array.Sort(uiObjs, (a, b) => a.Layer - b.Layer);
                
                /*
                foreach (var obj in meshObjs)
                {
                    var coms = obj.GetComponents<DrawableGameComponent>((c) => c.Enabled && !(c is MeshRenderer));
                    meshList.AddRange(coms);
                }
                foreach (var obj in beforeObjs)
                {
                    var coms = obj.GetComponents<DrawableGameComponent>((c) => c.Enabled && !(c is MeshRenderer));
                    beforeList.AddRange(coms);
                }
                foreach (var obj in afterObjs)
                {
                    var coms = obj.GetComponents<DrawableGameComponent>((c) => c.Enabled && !(c is MeshRenderer));
                    afterList.AddRange(coms);
                }
                foreach (var obj in uiObjs)
                {
                    var coms = obj.GetComponents<DrawableGameComponent>((c) => c.Enabled && !(c is MeshRenderer));
                    uiList.AddRange(coms);
                }
                */

                //meshComs = meshList.ToArray();
                beforeComs = beforeList.ToArray();
                afterComs = afterList.ToArray();
                uiComs = uiList.ToArray();
                //uiMeshComs = uiMeshList.ToArray();
            });

            // ライト情報を抽出
            var lights = new List<BufferLight>();
            var lightExtractTask = Task.Factory.StartNew(() =>
            {
                foreach (var obj in objects)
                {
                    var pls = obj.GetComponents<PointLight>();
                    foreach (var pl in pls)
                    {
                        var l = new BufferLight()
                        {
                            color = pl.Color.ToVector4(),
                            pos = new Vector4(pl.Transform.WorldPosition),
                            dir = -Vector4.UnitY,
                            intensity = pl.Intensity,
                            radius = pl.Radius,
                            specCoeff = pl.SpecularCoeff,
                            innerDot = -1.0f,
                            outerDot = -1.0f,
                        };
                        lights.Add(l);
                    }

                    var sls = obj.GetComponents<SpotLight>();
                    foreach (var sl in sls)
                    {
                        var l = new BufferLight()
                        {
                            color = sl.Color.ToVector4(),
                            pos = new Vector4(sl.Transform.WorldPosition),
                            dir = new Vector4(sl.WorldDirection, 0.0f),
                            intensity = sl.Intensity,
                            radius = sl.Radius,
                            specCoeff = sl.SpecularCoeff,
                            innerDot = sl.InnerDot,
                            outerDot = sl.OuterDot,
                        };
                        lights.Add(l);
                    }
                }
                lights = lights.Take(2048).ToList();
            });

            var clipDic = new Dictionary<GameObject, bool>();
            var clipTask = Task.Factory.StartNew(() =>
            {
                distTask.Wait();
                foreach (var obj in objects)
                {
                    var mr = obj.GetComponent<MeshRenderer>();
                    if (mr == null || !mr.Enabled)
                    {
                        clipDic.Add(obj, true);
                        continue;
                    }

                    if (mr.ForceRendering)
                    {
                        clipDic.Add(obj, false);
                        continue;
                    }

                    if (distDic[obj] < ForceRenderDistance)
                    {
                        clipDic.Add(obj, false);
                        continue;
                    }

                    var clip = true;
                    var b = mr.Mesh.Bounds;

                    var mvp = obj.Transform.WorldTransform * ViewProjection;
                    var mint = new Vector4(b.Min, 1.0f) * mvp;
                    var maxt = new Vector4(b.Max, 1.0f) * mvp;
                    var min = mint.Xyz / mint.W;
                    var max = maxt.Xyz / maxt.W;

                    var xx = new float[] { min.X, max.X };
                    var yy = new float[] { min.Y, max.Y };
                    var zz = new float[] { min.Z, max.Z };

                    foreach (var x in xx)
                    {
                        foreach (var y in yy)
                        {
                            foreach (var z in zz)
                            {
                                if (
                                x > -1.5f && x < 1.5f &&
                                y > -1.5f && y < 1.5f &&
                                z > -1.0f && z < 1.0f)
                                {
                                    clip = false;
                                    break;
                                }
                            }
                            if (!clip) break;
                        }
                        if (!clip) break;
                    }

                    clipDic.Add(obj, clip);
                }
            });

            var animTasks = new List<Task>();
            var findAnimTask = Task.Factory.StartNew(() =>
            {
                clipTask.Wait();
                distTask.Wait();
                foreach (var obj in objects)
                {
                    if (!obj.Enabled) continue;
                    if (clipDic[obj]) continue;

                    var animator = obj.GetComponent<AAnimator>();
                    var morpher = obj.GetComponent<AMorpher>();
                    
                    if (animator != null && animator.Enabled)
                    {
                        if (animator.EnableAsyncCalc)
                        {
                            animTasks.Add(Task.Run(() => animator.CalcTransform()));
                        }
                        animList.Add(animator);
                    }

                    if (morpher != null && morpher.Enabled && distDic[obj] < 10.0f)
                    {
                        if (morpher.EnableAsyncCalc)
                        {
                            animTasks.Add(Task.Run(() => morpher.CalcMorph()));
                        }
                        morphList.Add(morpher);
                    }
                }
            });

            List<MeshRenderer> renderers = new List<MeshRenderer>();
            if (MeshDraw)
            {
                Matrix4 proj;
                if (Orthographic) proj = Matrix4.CreateOrthographic(TargetTexture.Size.Width, TargetTexture.Size.Height, Near, Far);
                proj = Matrix4.CreatePerspectiveFieldOfView(FoV, Aspect, Near, Far);

                sp.pointLights = null;
                sp.spotLights = null;
                sp.camera = this;
                sp.resolution = new Vector2(TargetTexture.Size.Width, TargetTexture.Size.Height);
                sp.deltaTime = deltaTime;
                sp.oldViewProj = sp.viewProj;
                sp.view = View;
                sp.proj = proj;
                sp.cameraPos = wp;
                sp.cameraDir = WorldDirection;
                sp.dirLight = MMW.DirectionalLight;
                var viewproj = sp.view * sp.proj;
                sp.viewProj = viewproj;
                sp.viewInverse = sp.view.Inverted();
                sp.projInverse = sp.proj.Inverted();
                sp.viewProjInverse = sp.viewProj.Inverted();
                sp.environmentMap = EnvironmentMap;

                // 描画準備
                clipTask.Wait();

                var query = 0;
                foreach (var obj in objects)
                {
                    if (!obj.Enabled) continue;
                    if (clipDic[obj]) continue;

                    var rds = obj.GetComponents<MeshRenderer>();
                    renderers.AddRange(rds);

                    foreach (var r in rds)
                    {
                        r.query = queries[query++];
                    }
                }

                

                foreach (var obj in objects)
                {
                    if (!obj.Enabled) continue;
                    if (clipDic[obj]) continue;

                    var rds = obj.GetComponents<MeshRenderer>();
                    var animator = obj.GetComponent<AAnimator>();
                    var morpher = obj.GetComponent<AMorpher>();

                    var world = obj.Transform.WorldTransform;
                    var oldWorld = obj.Transform.OldWorldTransfom;
                    foreach (var r in rds)
                    {
                        for (var i = 0; i < r.MaterialCount; i++)
                        {
                            var mat = r.GetMaterialAt(i);
                            if (!drawMeshDic.ContainsKey(mat.Shader)) drawMeshDic.Add(mat.Shader, new Dictionary<Material, List<SubMeshData>>());
                            if (!drawMeshDic[mat.Shader].ContainsKey(mat)) drawMeshDic[mat.Shader].Add(mat, new List<SubMeshData>());
                        }
                        foreach (var sm in r.Mesh.subMeshes)
                        {
                            var mat = r.GetMaterial(sm.materialIndex);
                            var smd = new SubMeshData()
                            {
                                SubMesh = sm,
                                WorldTransform = world,
                                WorldTransformInv = world.Inverted(),
                                OldWorldTransform = oldWorld,
                                Visible = r.Visible,
                                CastShadow = r.CastShadow,
                                GameObject = obj,
                                Morpher = morpher,
                                Animator = animator,
                                OcclusionQuery = r.query,
                                MeshRenderer = r,
                            };
                            drawMeshDic[mat.Shader][mat].Add(smd);
                        }
                    }
                }

                findAnimTask.Wait();
                foreach (var task in animTasks) task.Wait();
                foreach (var a in animList)
                {
                    if (!a.EnableAsyncCalc) a.CalcTransform();
                    a.UpdateData();
                }
                foreach (var m in morphList)
                {
                    if (!m.EnableAsyncCalc) m.CalcMorph();
                    m.UpdateData();
                } 

                // 影の準備
                if (ShadowMapping && MMW.Configuration.ShadowQuality != MMWConfiguration.ShadowQualityType.NoShadow)
                {
                    Vector3 lightDir = MMW.DirectionalLight.WorldDirection;
                    Vector3 center = wp;
                    Matrix4 view = Matrix4.Identity;
                    Matrix4 projo = Matrix4.Identity;

                    // render to far shadow depth map
                    /*
                    {
                        view = Matrix4.LookAt(Vector3.Zero, lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(240, 240, -150, 150);
                        sp.viewProj = view * projo;
                        if (prevLightDir != lightDir)
                        {
                            RenderToDepthMap(shadowDepthRT1, sp, drawMeshDic);
                        }
                        sp.shadowDepthBias1 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap1 = shadowDepthRT1.ColorDst0;
                    }*/

                    //  render to middle shadow depth map
                    {
                        view = Matrix4.LookAt(center, center + lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(40, 40, -150, 150);
                        sp.viewProj = view * projo;
                        RenderToDepthMap(shadowDepthRT2, sp, drawMeshDic);
                        sp.shadowDepthBias2 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap2 = shadowDepthRT2.ColorDst0;
                    }

                    //  render to near shadow depth map
                    {
                        view = Matrix4.LookAt(center, center + lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(10, 10, -80, 80);
                        sp.viewProj = view * projo;
                        RenderToDepthMap(shadowDepthRT3, sp, drawMeshDic);
                        sp.shadowDepthBias3 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap3 = shadowDepthRT3.ColorDst0;
                    }

                    sp.viewProj = viewproj;
                    prevLightDir = lightDir;
                    prevCameraPos = center;
                }
            }

            // 遅延描画
            deferredRT.Bind(new Color4(0, 0, 0, 0));

            // スカイボックス
            if (ClearFlag == ClearFlag.SkyBox)
            {
                var dir = GameObject.Transform.WorldDirectionZ;
                if (ForceTarget) dir = (Target - wp).Normalized();

                var mat = Matrix4.LookAt(Vector3.Zero, dir, Up) * Projection;
                var oldMat = Matrix4.LookAt(Vector3.Zero, prevCameraDir, Up) * Projection;

                GL.Disable(EnableCap.DepthTest);
                backShader.UseShader();
                backShader.SetParameter(backShader.loc_mvp, ref mat, false);
                backShader.SetParameter(backShader.loc_oldmvp, ref oldMat, false);
                backShader.SetParameter(backShader.loc_albedo, SkyBoxColor);
                backShader.SetParameter(backShader.loc_fog, MMW.FogIntensity);
                backShader.SetParameter(backShader.loc_fogcolor, MMW.MainCamera.ClearColor);
                backShader.SetParameter(TextureUnit.Texture0, EnvironmentMap);
                Drawer.DrawSubMesh(cubeMesh.subMeshes[0]);
                backShader.UnuseShader();
                GL.Enable(EnableCap.DepthTest);

                prevCameraDir = dir;
            }

            // その他スクリプトの描画(メッシュ描画前)
            if (ScriptDraw)
            {
                layerSortTask.Wait();
                DrawScripts(beforeComs, (float)deltaTime);
                DrawMeshScripts(beforeComs, (float)deltaTime);
            }

            if (MeshDraw)
            {
                // オクルージョン描画
                //DrawOcclusionBox(renderers);
                DrawDeferredMeshes(sp, drawMeshDic);
                //DrawMeshScripts(meshComs, (float)deltaTime);
            }

            // その他スクリプトの描画
            if (ScriptDraw)
            {
                DrawScripts(afterComs, (float)deltaTime);
                DrawMeshScripts(afterComs, (float)deltaTime);
            }

            // ライトカリング
            lightExtractTask.Wait();
            lightRT.Bind(new Color4(0, 0, 0, 0));
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_light);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, lights.Count * 20 * 4, lights.ToArray());
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_count);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, initCounts.Length * 4, initCounts);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            lightCullingShader.UseShader();
            AAnimator.BindIdentity(1, 2);
            GL.CullFace(CullFaceMode.Back);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, ssbo_count);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, ssbo_index);
            lightCullingShader.SetParameter(lightCullingShader.loc_resolution, lightRT.Size.ToVector2());
            var syncList = new List<IntPtr>();
            for (var i = 0; i < lights.Count; i++)
            {
                lightCullingShader.SetParameter(lightCullingShader.loc_index, i);
                var mvp = MatrixHelper.CreateTransform(lights[i].pos.Xyz, Vector3.Zero, new Vector3(lights[i].radius * 1.2f)) * sp.viewProj;
                lightCullingShader.SetParameter(lightCullingShader.loc_mvp, ref mvp, false);
                syncList.Add(GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None));
                Drawer.DrawSubMesh(plMesh.subMeshes[0]);
            }
            GL.CullFace(CullFaceMode.Front);
            lightCullingShader.UnuseShader();

            foreach (var sync in syncList)
            {
                GL.DeleteSync(sync);
            }

            // レンダーターゲットをセット
            TargetTexture.Bind();
            GL.ClearColor(ClearColor);
            if (ClearFlag == ClearFlag.SolidColor) GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            else if (ClearFlag == ClearFlag.DepthOnly) GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            else if (ClearFlag == ClearFlag.SkyBox)
            {
                GL.ClearColor(Color4.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            } 

            // メッシュ描画
            if (MeshDraw)
            {
                /*
                var srcRect = new RectangleF(0, 0, 1.0f, 1.0f);
                var dstRect1 = new RectangleF(0, 0, 0.25f, 0.5f);
                var dstRect2 = new RectangleF(0, 0.5f, 0.25f, 0.5f);
                var dstRect3 = new RectangleF(0.25f, 0, 0.25f, 0.5f);
                var dstRect4 = new RectangleF(0.25f, 0.5f, 0.25f, 0.5f);
                var dstRect5 = new RectangleF(0.5f, 0, 0.25f, 0.5f);
                var dstRect6 = new RectangleF(0.5f, 0.5f, 0.25f, 0.5f);
                var dstRect7 = new RectangleF(0.75f, 0, 0.25f, 0.5f);
                var dstRect8 = new RectangleF(0.75f, 0.5f, 0.25f, 0.5f);
                Drawer.DrawTexture(sp.shadowDepthMap2, srcRect, dstRect1);
                Drawer.DrawTexture(sp.shadowDepthMap3, srcRect, dstRect2);
                Drawer.DrawTexture(deferredRT.ColorDst2, srcRect, dstRect3);
                Drawer.DrawTexture(deferredRT.ColorDst3, srcRect, dstRect4);
                Drawer.DrawTexture(deferredRT.ColorDst4, srcRect, dstRect5);
                Drawer.DrawTexture(deferredRT.ColorDst5, srcRect, dstRect6);
                Drawer.DrawTexture(deferredRT.ColorDst6, srcRect, dstRect7);
                Drawer.DrawTexture(deferredRT.ColorDst7, srcRect, dstRect8);
                */
                //Drawer.DrawTexture(lightRT.ColorDst0);
                
                sp.ortho = orthoMat;
                sp.deferredAlbedoMap = deferredRT.ColorDst0;
                sp.deferredWorldPosMap = deferredRT.ColorDst1;
                sp.deferredWorldNormalMap = deferredRT.ColorDst2;
                sp.deferredPhysicalParamsMap = deferredRT.ColorDst3;
                sp.deferredF0Map = deferredRT.ColorDst4;
                sp.deferredDepthMap = deferredRT.ColorDst5;
                sp.deferredShadowMap = deferredRT.ColorDst6;
                sp.deferredVelocityMap = deferredRT.ColorDst7;

                deferredShader.UseShader();
                deferredShader.SetUniqueParameter(sp, true);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, ssbo_light);
                Drawer.DrawTextureMesh();
                deferredShader.UnuseShader();

                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, 0);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, 0);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, 0);
            }

            // イメージエフェクトをかける
            var effects = GameObject.GetComponents<ImageEffect>();
            if (effects != null)
            {
                for (var i = 0; i < effects.Length; i++)
                {
                    if (!effects[i].Enabled) continue;
                    if (effects[i].RequireMaps.Contains(RequireMap.Depth)) effects[i].DepthMap = sp.deferredDepthMap;
                    if (effects[i].RequireMaps.Contains(RequireMap.Position)) effects[i].PositionMap = sp.deferredWorldPosMap;
                    if (effects[i].RequireMaps.Contains(RequireMap.Normal)) effects[i].NormalMap = sp.deferredWorldNormalMap;
                    if (effects[i].RequireMaps.Contains(RequireMap.Velocity)) effects[i].VelocityMap = sp.deferredVelocityMap;
                    effects[i].Draw(deltaTime);
                }
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);

            // UI層のゲームオブジェクトを描画
            if (UIDraw && layerMask.Get(GameObject.LayerUI))
            {
                DrawScripts(uiComs, (float)deltaTime);
                DrawMeshScripts(uiComs, (float)deltaTime);
            }
        }

        protected void DrawOcclusionBox(List<MeshRenderer> rds)
        {
            colorShader.UseShader();
            AAnimator.BindIdentity(1);
            colorShader.SetParameter(colorShader.loc_color, new Color4(0.0f, 0.0f, 0.0f, 0.0f));

            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(false);
            foreach (var mr in rds)
            {
                var m = MatrixHelper.CreateTransform(mr.Mesh.Bounds.Center, Vector3.Zero, mr.Mesh.Bounds.Extents);
                m *= mr.GameObject.Transform.WorldTransform;
                m *= ViewProjection;
                colorShader.SetParameter(colorShader.loc_mvp, ref m, false);

                GL.BeginQuery(QueryTarget.SamplesPassed, mr.query);
                Drawer.DrawSubMesh(boundsMesh.subMeshes[0]);
                GL.EndQuery(QueryTarget.SamplesPassed);
            }
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            
            colorShader.UnuseShader();
        }

        protected void DrawOcclusionBox(ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            colorShader.UseShader();
            AAnimator.BindIdentity(1);

            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);
            foreach (var sh in sm)
            {
                foreach (var mat in sh.Value)
                {
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible) continue;

                        sp.world = sub.WorldTransform;
                        colorShader.SetUniqueParameter(sp, false);

                        GL.BeginQuery(QueryTarget.SamplesPassed, sub.OcclusionQuery);
                        //Drawer.DrawSubMesh(sub.SubMesh.boundsMesh.subMeshes[0]);
                        GL.EndQuery(QueryTarget.SamplesPassed);
                        
                    }
                }
            }
            GL.Enable(EnableCap.CullFace);
            GL.DepthMask(true);

            colorShader.UnuseShader();
        }

        protected void DrawDeferredMeshes(ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            foreach (var sh in sm)
            {
                sh.Key.UseShader();
                sh.Key.SetUniqueParameter(sp, true);
                foreach (var mat in sh.Value)
                {
                    mat.Key.ApplyShaderParam();
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible) continue;

                        //int result;
                        //GL.GetQueryObject(sub.OcclusionQuery, GetQueryObjectParam.QueryResult, out result);
                        //if (result == 0)
                        //{
                        //    continue;
                        //} 


                        sp.world = sub.WorldTransform;
                        sp.worldInv = sub.WorldTransformInv;
                        sp.oldWorld = sub.OldWorldTransform;

                        sh.Key.SetUniqueParameter(sp, false);

                        if (sub.Morpher != null) sub.Morpher.UseMorph(0);
                        if (sub.Animator != null) sub.Animator.BindMotion(1, 2);
                        else AAnimator.BindIdentity(1, 2);

                        Drawer.DrawSubMesh(sub.SubMesh, sub.MeshRenderer.BeginMode);

                        if (sub.Animator != null) sub.Animator.UnbindMotion(1, 2);
                        else AAnimator.UnbindIdentity(1, 2);
                        if (sub.Morpher != null) sub.Morpher.UnuseMorph(0);
                    }
                }
                sh.Key.UnuseShader();
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        protected void DrawMeshScripts(DrawableGameComponent[] coms, float deltaTime)
        {
            for (var i = 0; i < coms.Length; i++)
            {
                coms[i].MeshDraw(deltaTime, this);
            }
        }

        protected void DrawScripts(DrawableGameComponent[] coms, float deltaTime)
        {
            Drawer.ClearGraphics();

            for (var i = 0; i < coms.Length; i++)
            {
                coms[i].Draw(deltaTime, this);
            }

            if (DebugDraw)
            {
                for (var i = 0; i < coms.Length; i++)
                {
                    coms[i].DebugDraw(deltaTime, this);
                }
            }

            Drawer.EndGraphicsDraw();
        }

        protected override void CreateShadowMap()
        {
            var middle = new Size(2048, 2048);
            var near = new Size(1024, 1024);

            if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.VeryHigh)
            {
                near = middle;
                middle = new Size(4096, 4096);
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.Default)
            {
                //far = middle;
                //middle = near;
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.Low)
            {
                //far = near;
                middle = near;
                near = new Size(512, 512);
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.VeryLow)
            {
                //far = new Size(512, 512);
                middle = new Size(1024, 1024);
                near = new Size(256, 256);
            }

            shadowDepthRT2 = new RenderTexture(middle);
            shadowDepthRT2.ColorFormat0 = PixelInternalFormat.Rgba32f;
            shadowDepthRT2.MagFilter = TextureMagFilter.Linear;
            shadowDepthRT2.Load();

            shadowDepthRT3 = new RenderTexture(near);
            shadowDepthRT3.ColorFormat0 = PixelInternalFormat.Rgba32f;
            shadowDepthRT3.MagFilter = TextureMagFilter.Linear;
            shadowDepthRT3.Load();
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (AutoResize && message == MMWSettings.Default.Message_WindowResize)
            {
                Width = MMW.RenderResolution.Width;
                Height = MMW.RenderResolution.Height;
            }
        }
    }
}
