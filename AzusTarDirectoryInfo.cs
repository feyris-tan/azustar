using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzusTar
{
    public class AzusTarDirectoryInfo
    {
        internal AzusTarDirectoryInfo(string name, AzusTarDirectoryInfo parent)
        {
            this._name = name;
            this._parent = parent;
            if (this._parent != null)
                this._parent.AddChild(this);
            this.children = new List<AzusTarDirectoryInfo>();
            this.files = new List<AzustarFileInfo>();
        }

        private string _name;
        private AzusTarDirectoryInfo _parent;
        private List<AzusTarDirectoryInfo> children;
        private List<AzustarFileInfo> files;

        private void AddChild(AzusTarDirectoryInfo child)
        {
            children.Add(child);
        }

        internal AzusTarDirectoryInfo GetOrAddChild(string name)
        {
            foreach (AzusTarDirectoryInfo child in children)
            {
                if (child._name.Equals(name))
                    return child;
            }

            AzusTarDirectoryInfo baby = new AzusTarDirectoryInfo(name, this);
            return baby;
        }

        internal void AddFile(AzustarFileInfo file)
        {
            this.files.Add(file);
        }

        public override string ToString()
        {
            if (_parent == null)
                return "";

            return String.Format("{0}{1}/", _parent.ToString(), _name);
        }

        public IReadOnlyList<AzusTarDirectoryInfo> Subdirectories => children.AsReadOnly();
        public IReadOnlyList<AzustarFileInfo> Files => files.AsReadOnly();
        public string Name => this._name;
    }
}
