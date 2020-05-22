﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Xamarin.Forms.Internals;

namespace Sharpnado.Shades.Droid
{
    public partial class ShadowView
    {
        private float _cornerRadius;
        private IEnumerable<Shade> _shadesSource;

        public void UpdateCornerRadius(float cornerRadius)
        {
            bool hasChanged = _cornerRadius != cornerRadius;
            _cornerRadius = cornerRadius;

            if (hasChanged && _shadesSource.Any())
            {
                var immutableSource = _shadesSource.ToArray();
                DrawBitmaps(immutableSource);
                Invalidate();
            }
        }

        public void UpdateShades(IEnumerable<Shade> shadesSource)
        {
            if (shadesSource == null)
            {
                return;
            }

            if (_shadesSource is INotifyCollectionChanged previousNotifyCollectionChanged)
            {
                previousNotifyCollectionChanged.CollectionChanged -= ShadesSourceCollectionChanged;
            }

            _shadesSource = shadesSource;
            if (_shadesSource is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += ShadesSourceCollectionChanged;
            }

            DisposeBitmaps();
            for (int i = 0; i < _shadesSource.Count(); i++)
            {
                InsertShade(i, _shadesSource.ElementAt(i));
            }
        }

        private void ShadesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0, insertIndex = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        InsertShade(insertIndex, (Shade)e.NewItems[i]);
                    }

                    Invalidate();

                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0, removedIndex = e.OldStartingIndex; i < e.OldItems.Count; i++)
                    {
                        RemoveShade(removedIndex, (Shade)e.OldItems[i]);
                    }

                    Invalidate();

                    break;

                case NotifyCollectionChangedAction.Reset:
                    DisposeBitmaps();
                    Invalidate();
                    break;
            }
        }

        private void InsertShade(int insertIndex, Shade shade)
        {
            CreateBitmap(insertIndex);
            DrawBitmap(insertIndex, shade);
            shade.PropertyChanged += ShadePropertyChanged;
        }

        private void RemoveShade(int removedIndex, Shade shade)
        {
            shade.PropertyChanged -= ShadePropertyChanged;
            DisposeBitmap(removedIndex);
        }

        private void ShadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var shade = (Shade)sender;
            var index = _shadesSource.IndexOf(shade);
            if (index < 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ShadowView::ShadePropertyChanged => shade property {e.PropertyName} changed but we can't find the shade in the source");
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(Shade.BlurRadius):
                case nameof(Shade.Color):
                    DrawBitmap(index, shade);
                    Invalidate();
                    break;

                case nameof(Shade.Offset):
                    Invalidate();
                    break;
            }
        }
    }
}