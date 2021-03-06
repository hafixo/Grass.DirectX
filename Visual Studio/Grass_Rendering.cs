﻿using System.Text;
using GrassRendering.Components;
using GrassRendering.Core;
using GrassRendering.Environment;
using GrassRendering.Grass;
using SharpDX;
using SharpDX.Direct3D11;
using Color = SharpDX.Color;

namespace GrassRendering
{
    // Use these namespaces here to override SharpDX.Direct3D11
    using SharpDX.Toolkit;
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    /// <summary>
    /// The application's main class.
    /// </summary>
    public class GrassRenderingApplication : Game
    {
        #region Fields

        // DirectX
        private readonly GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;
        private SpriteFont arial16Font;

        // Engine
        private GameCore gameCore;

        // Camera
        private Camera camera;
        private ShadowCamera shadowCamera;

        // Objects
        private Terrain terrain;
        private GrassController grass;
        private SkyBox skyBox;

        // Input
        private readonly KeyboardManager keyboard;
        private KeyboardState keyboardState;
        private readonly MouseManager mouse;
        private MouseState mouseState;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GrassRenderingApplication" /> class.
        /// </summary>
        public GrassRenderingApplication()
        {
            this.graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 3840,
                PreferredBackBufferHeight = 2400,
                PreferMultiSampling = true,
                IsFullScreen = true,
                SynchronizeWithVerticalRetrace = false,
            };

            Content.RootDirectory = "Content";

            // Input
            this.keyboard = new KeyboardManager(this);
            this.mouse = new MouseManager(this);
        }

        /// <summary>
        /// Called after the Game and GraphicsDevice are created, but before LoadContent.  Reference page contains code sample.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = "GrassRenderingApplication";
            base.Initialize();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            // Instantiate a SpriteBatch
            spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));

            arial16Font = Content.Load<SpriteFont>("Fonts/Arial16");
            camera = new Camera(this.GraphicsDevice, this.graphicsDeviceManager.PreferredBackBufferWidth, this.graphicsDeviceManager.PreferredBackBufferHeight, keyboard, mouse);
            shadowCamera = new ShadowCamera(this.graphicsDeviceManager.PreferredBackBufferWidth, this.graphicsDeviceManager.PreferredBackBufferHeight);

            gameCore = new GameCore(this.GraphicsDevice, this.Content, this.camera, this.shadowCamera, this.keyboard, this.mouse);

            terrain = new Terrain(this.gameCore);
            grass = new GrassController(this.gameCore);
            skyBox = new SkyBox(this.gameCore);
            base.LoadContent();
        }

        /// <summary>
        /// Reference page contains links to related conceptual articles.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to Update.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            gameCore.Update();
            keyboardState = keyboard.GetState();
            mouseState = mouse.GetState();
            camera.Update(gameTime, this.IsActive);
            grass.Update(gameTime);
        }

        /// <summary>
        /// Reference page contains code sample.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            this.SetUpBlendState();
            this.SetUpRasterizerState();

            // Input
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Objects
            skyBox.Draw(camera);
            terrain.Draw(camera);
            grass.Draw(gameTime, camera);

            // Draw string (Mouse position and FPS)
            spriteBatch.Begin();
            var text = new StringBuilder("");
            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            text.AppendFormat("FPS: {0}", frameRate).AppendLine();
            spriteBatch.DrawString(arial16Font, text.ToString(), new Vector2(16, 16), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the blending state.
        /// </summary>
        private void SetUpBlendState()
        {
            var blendStateDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true
            };

            blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.DestinationAlpha;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;

            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.DestinationAlpha;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.ReverseSubtract;

            BlendState blendState = BlendState.New(this.GraphicsDevice, blendStateDesc);
            this.GraphicsDevice.SetBlendState(blendState);
        }

        /// <summary>
        /// Sets the state of the rasterizer.
        /// </summary>
        private void SetUpRasterizerState()
        {
            RasterizerStateDescription stateDescription = new RasterizerStateDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None
            };

            this.GraphicsDevice.SetRasterizerState(RasterizerState.New(this.GraphicsDevice, stateDescription));
        }

        #endregion
    }
}
