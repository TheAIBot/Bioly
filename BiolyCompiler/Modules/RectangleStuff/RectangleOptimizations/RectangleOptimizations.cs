using BiolyCompiler.Architechtures;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules.RectangleStuff.RectangleOptimizations
{
    public static class RectangleOptimizations
    {
        public static void OptimizeRectangle(Board board, Rectangle firstToOptimize)
        {
            Queue<Rectangle> rectanglesToOptimize = new Queue<Rectangle>();
            rectanglesToOptimize.Enqueue(firstToOptimize);

            while (rectanglesToOptimize.Count > 0)
            {
                Rectangle toOptimize = rectanglesToOptimize.Dequeue();

                //It may be the case that the rectangle has been removed from the board
                //while it stayed in the queue and it that case it should just be ignored
                if (!board.EmptyRectangles.ContainsKey(toOptimize))
                {
                    continue;
                }

                Rectangle optimizedRectangle = MergeRectanglesOptimization.TryMergeOptimization(board, toOptimize);
                if (optimizedRectangle != null)
                {
                    rectanglesToOptimize.Enqueue(optimizedRectangle);
                    continue;
                }

                Rectangle[] optimizedRectangles = RectangleLSplitOptimization.SplitMerge(board, toOptimize);
                if (optimizedRectangles != null)
                {
                    optimizedRectangles.ForEach(x => rectanglesToOptimize.Enqueue(x));
                    continue;
                }
            }
        }
    }
}
