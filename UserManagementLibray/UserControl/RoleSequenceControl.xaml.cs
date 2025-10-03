using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;
using UserManagementLibray.Entity;

namespace UserManagementLibray
{
    public partial class RoleSequenceControl : UserControl
    {
        private Point _startPoint;
        private RoleNode _selectedNode;
        private List<RoleNode> _roleTree;

        public RoleSequenceControl()
        {
            InitializeComponent();
            LoadRoleTree();
        }

        private void LoadRoleTree()
        {
            var roles = RoleRepository.GetActiveRoleBasedOnSequence();
            _roleTree = BuildRoleTree(roles);
            RoleTreeView.ItemsSource = _roleTree;
        }

        private List<RoleNode> BuildRoleTree(List<Role> roles, int? parentId = null)
        {
            var roleList =  roles.Where(r => r.ParentRoleID == parentId).OrderBy(r => r.PriorityIndex).Select(r => new RoleNode
                {
                    RoleId = r.RoleID,
                    Role_Name = r.Role_Name,
                    PriorityIndex = r.PriorityIndex,
                    ParentRoleID = r.ParentRoleID,
                    Children = BuildRoleTree(roles, r.RoleID),
                    IsExpanded = true
                }).ToList();
            return roleList;
        }

        private void RoleTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedNode = e.NewValue as RoleNode;
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode == null) return;
            if (_selectedNode.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("System Administrator role cannot be moved.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var parent = FindParent(_roleTree, _selectedNode);
            var siblings = parent?.Children ?? _roleTree;

            int index = siblings.IndexOf(_selectedNode);
            if (siblings[index - 1].Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("System Administrator role cannot be moved.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (index > 0)
            {
                var temp = siblings[index - 1];
                siblings[index - 1] = _selectedNode;
                siblings[index] = temp;
                //UpdatePriorityIndexes(siblings);

                RoleTreeView.Items.Refresh();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNode == null) return;
            if (_selectedNode.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("System Administrator role cannot be moved.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var parent = FindParent(_roleTree, _selectedNode);
            var siblings = parent?.Children ?? _roleTree;

            int index = siblings.IndexOf(_selectedNode);
            if (siblings[index + 1].Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("System Administrator role cannot be moved.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (index < siblings.Count - 1)
            {
                var temp = siblings[index + 1];
                siblings[index + 1] = _selectedNode;
                siblings[index] = temp;
              //  UpdatePriorityIndexes(siblings);

                RoleTreeView.Items.Refresh();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveTreeToDatabase(_roleTree);
            MessageBox.Show("Changes saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveTreeToDatabase(List<RoleNode> nodes, int? parentId = null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.ParentRoleID = parentId;
                node.PriorityIndex = i + 1;

                RoleRepository.UpdatePriorityIndex(node.RoleId, node.PriorityIndex);
                RoleRepository.UpdateParentRole(node.RoleId, node.ParentRoleID);

                if (node.Children.Count > 0)
                {
                    SaveTreeToDatabase(node.Children, node.RoleId);
                }
            }
        }

        private RoleNode FindParent(List<RoleNode> nodes, RoleNode target)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Contains(target))
                    return node;

                var found = FindParent(node.Children, target);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void UpdatePriorityIndexes(List<RoleNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].PriorityIndex = i + 1;
                RoleRepository.UpdatePriorityIndex(nodes[i].RoleId, nodes[i].PriorityIndex);
            }
        }

        private void RefreshTree()
        {
            var roles = RoleRepository.GetActiveRoleBasedOnSequence();
            _roleTree = BuildRoleTree(roles);
            RoleTreeView.ItemsSource = null;
            RoleTreeView.ItemsSource = _roleTree;
        }

        #region Drag & Drop

        private void RoleTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void RoleTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                if ((Math.Abs(currentPosition.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(currentPosition.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    if (_selectedNode != null)
                        DragDrop.DoDragDrop(RoleTreeView, _selectedNode, DragDropEffects.Move);
                }
            }
        }

        private void RoleTreeView_Drop(object sender, DragEventArgs e)
        {
            var targetNode = ((FrameworkElement)e.OriginalSource)?.DataContext as RoleNode;
            var draggedNode = e.Data.GetData(typeof(RoleNode)) as RoleNode;

            if (draggedNode == null) return;

            // Prevent making anything a child of System Administrator
            if (targetNode != null && targetNode.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("You cannot make any role a child of System Administrator.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prevent moving System Administrator itself
            if (draggedNode.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("System Administrator role cannot be moved.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var oldParent = FindParent(_roleTree, draggedNode);
            var siblingsOld = oldParent?.Children ?? _roleTree;

            bool removed = siblingsOld.Remove(draggedNode);
            if (!removed) return;

            if (!IsDropOnNode(e) || targetNode == null || targetNode == draggedNode)
            {
                // Dropped on empty space → make root node
                draggedNode.ParentRoleID = null;
                draggedNode.Children.Clear();
                _roleTree.Add(draggedNode);
            }
            else
            {
                // Enforce only two levels: targetNode can have children, but its children cannot
                if (targetNode.ParentRoleID != null)
                {
                    MessageBox.Show("You cannot add a child to this role because only two levels are allowed.", "Operation Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    siblingsOld.Add(draggedNode);
                    return;
                }

                draggedNode.ParentRoleID = targetNode.RoleId;
                draggedNode.Children.Clear(); // No grandchildren
                targetNode.Children.Add(draggedNode);
            }

            UpdatePriorityIndexes(siblingsOld);
            RoleTreeView.Items.Refresh();
        }



        private bool IsDropOnNode(DragEventArgs e)
        {
            var pos = e.GetPosition(RoleTreeView);
            var result = VisualTreeHelper.HitTest(RoleTreeView, pos);
            return result?.VisualHit is FrameworkElement fe && fe.DataContext is RoleNode;
        }

        #endregion
    }

   
}
