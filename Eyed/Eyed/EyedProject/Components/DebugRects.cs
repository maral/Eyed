using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;

namespace EyedProject.Components
{
    class DebugRects : Drawable2D
    {
        private List<RectangleF> rectangles;
        public DebugRects(List<RectangleF> rects)
            : base("debugRects", DefaultLayers.Alpha)
        {
            rectangles = rects;
        }
        public override void Draw(TimeSpan gameTime)
        {
            foreach (var rect in rectangles)
            {
                RenderManager.LineBatch2D.DrawRectangle(rect, Color.Yellow, 0f);
            }
        }

        protected override void Dispose(bool disposing)
        {
 	        
        }
    }
}
