using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace RozkładJazdyv2.Model
{
    static class AnimationHelper
    {

        public static void CraeteFadeInAnimation(DependencyObject target, double duration)
            => CreateStoryBoardAnimation(
                CreateDoubleAnimation(new Duration(TimeSpan.FromSeconds(duration)), 0.0, 1.0))
                .SetStoryBoardTargetAndProperty(target, "Opacity").Begin();
   
        private static DoubleAnimation CreateDoubleAnimation(Duration duration, double from, double to)
            => new DoubleAnimation() { Duration = duration, From = from, To = to };

        private static Storyboard CreateStoryBoardAnimation(DoubleAnimation animation)
        {
            Storyboard storyBoard = new Storyboard() { Duration = animation.Duration };
            storyBoard.Children.Add(animation);
            return storyBoard;
        }
    }
}
