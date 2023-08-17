using System.Collections.ObjectModel;
namespace EMV_Card_Browser
{
    public class TreeNode
    {
        public string Name { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }
        public byte[] Data { get; set; }

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
        }
    }
}