using System.Collections;
using System.Collections.Generic;

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

        public IEnumerator<MediaPickerItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }

}