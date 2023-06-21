using System.Collections;
using System.Collections.Generic;

namespace Limbo.Umbraco.Migrations.Models.UrlPickerItem {

    public class UrlPickerList : IReadOnlyList<UrlPickerItem> {

        private readonly List<UrlPickerItem> _items = new();

        public int Count => _items.Count;

        public UrlPickerItem this[int index] => _items[index];

        public UrlPickerList() { }

        public UrlPickerList(UrlPickerItem item) {
            Add(item);
        }

        public void Add(UrlPickerItem item) {
            _items.Add(item);
        }

        public IEnumerator<UrlPickerItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }

}
