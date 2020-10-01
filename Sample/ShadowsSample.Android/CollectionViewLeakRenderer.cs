using Android.Content;

using ShadowsSample.Droid;

using Sharpnado.Presentation.Forms.Droid.Helpers;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CollectionView), typeof(CollectionViewLeakRenderer))]

namespace ShadowsSample.Droid
{
    public class CollectionViewLeakRenderer : CollectionViewRenderer
    {
        public CollectionViewLeakRenderer(Context context)
            : base(context)
        {
        }

        //protected override void Dispose(bool disposing)
        //{
        //    System.Diagnostics.Debug.WriteLine(ItemsViewAdapter);

        //    if (!disposing)
        //    {
        //        base.Dispose(false);
        //        return;
        //    }

        //    var children = GetChildren();
        //    base.Dispose(true);
        //    DisposeChildren(children);
        //}

        private ItemContentView[] GetChildren()
        {
            System.Diagnostics.Debug.WriteLine($"GetChildren() => count: {ChildCount}");

            
            var result = new ItemContentView[ChildCount];
            for (int childCount = ChildCount, i = 0; i < childCount; ++i)
            {
                result[i] = (ItemContentView)GetChildAt(i);
            }

            return result;
        }

        private void DisposeChildren(ItemContentView[] children)
        {
            System.Diagnostics.Debug.WriteLine($"DisposeChildren() => count: {children.Length}");
            for (int childCount = children.Length, i = 0; i < childCount; ++i) 
            {
                var itemContentView = children[i];
                if (!itemContentView.IsNullOrDisposed())
                {
                    // System.Diagnostics.Debug.WriteLine(itemContentView.DumpHierarchy(true));
                    var view = itemContentView.GetChildAt(0);
                    if (!view.IsNullOrDisposed())
                    {
                        view.Dispose();
                    }

                    itemContentView.RemoveAllViews();
                    itemContentView.Dispose();
                }
            }
        }
    }
}