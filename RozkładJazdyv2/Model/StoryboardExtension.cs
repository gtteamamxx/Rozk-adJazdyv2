using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace RozkładJazdyv2.Model
{
    internal static class StoryboardExtension
    {
        internal static Storyboard SetStoryBoardTargetAndProperty(this Storyboard storyBoard, DependencyObject target, string property)
        {
            Storyboard.SetTarget(storyBoard, target);
            Storyboard.SetTargetProperty(storyBoard, new PropertyPath(property).Path);
            return storyBoard;
        }
    }
}
