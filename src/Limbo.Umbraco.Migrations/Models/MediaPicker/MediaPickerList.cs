using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Migrations.Models.MediaPicker {

    public class MediaPickerList : IReadOnlyList<MediaPickerItem> {

        private readonly List<MediaPickerItem> _items = new();

        public int Count => _items.Count;

        public MediaPickerItem this[int index] => _items[index];

        public MediaPickerList() { }

        public MediaPickerList(MediaPickerItem item) {
            Add(item);
        }

        public void Add(MediaPickerItem item) {
            _items.Add(item);
        }

        public MediaPickerList AddItem(Guid itemKey, Guid mediaKey) {
            Add(new MediaPickerItem(itemKey, mediaKey));
            return this;
        }

        public MediaPickerList AddItem(Guid itemKey, IMedia media) {
            Add(new MediaPickerItem(itemKey, media.Key));
            return this;
        }

        public IEnumerator<MediaPickerItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public static MediaPickerList Create() {
            return new MediaPickerList();
        }

        public static MediaPickerList CreateWithItem(Guid itemKey, Guid mediaKey) {
            return new MediaPickerList().AddItem(itemKey, mediaKey);
        }

        public static MediaPickerList CreateWithItem(Guid itemKey, IMedia media) {
            return new MediaPickerList().AddItem(itemKey, media);
        }

    }

}