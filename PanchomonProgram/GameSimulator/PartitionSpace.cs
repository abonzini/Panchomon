namespace GameSimulator
{
    internal class PartitionSpace
    {
        int _nextId = 0;
        readonly Dictionary<int, Partition> _partitions = [];
        readonly Dictionary<int, int> _setParenting = []; // Links any partition with the macro disjoint set linked to it (ideally they'd all have the same value as all linked)
        /// <summary>
        /// Adds a new partition to the scheme
        /// </summary>
        /// <param name="partition">Which partition to add</param>
        /// <param name="id">Manual ID if needs to be specific</param>
        public void AddNewPartition(Partition partition, int id = -1)
        {
            int nextId = id == -1 ? _nextId : id;
            if (nextId < _nextId)
            {
                throw new Exception("Partition ID is lower than the internal counter and may be already occupied");
            }
            _nextId = nextId + 1; // The next one becomes the min possible
            partition.Id = nextId;
            _partitions.Add(nextId, partition);
            _setParenting.Add(nextId, nextId); // Each party begins as it's own disjoint set to be bridged later if needed
        }
        /// <summary>
        /// Removes a partition from this, mostly for when it's broken further
        /// </summary>
        /// <param name="partition"></param>
        public void RemovePartition(Partition partition)
        {
            _partitions.Remove(partition.Id);
            _setParenting.Remove(partition.Id);
        }
        /// <summary>
        /// Merges the partition space with the current one as the main one, shift the ids so they don't overlap
        /// </summary>
        /// <param name="other">New space to combine with this one</param>
        public void MergeSpace(PartitionSpace other)
        {
            int idBase = _nextId;
            int maxId = idBase;
            foreach (KeyValuePair<int, Partition> kvp in other._partitions) // Unify partitions
            {
                int newId = idBase + kvp.Key;
                if (newId > maxId) maxId = newId;
                kvp.Value.Id = newId;
                _partitions.Add(newId, kvp.Value);
            }
            foreach (KeyValuePair<int, int> set in other._setParenting) // Unify disjoint sets
            {
                _setParenting.Add(set.Key + idBase, set.Value + idBase);
            }
            _nextId = maxId + 1; // Ok if need to add more, prepare the id
        }
        /// <summary>
        /// Gets how many child partitions are here
        /// </summary>
        /// <returns>How many</returns>
        public int GetPartitionCount()
        {
            return _partitions.Count;
        }
        /// <summary>
        /// Gets a specific partition given its id
        /// </summary>
        /// <param name="id">Id of part</param>
        /// <returns>The partition that we look for</returns>
        public Partition GetPartition(int id)
        {
            return _partitions[id];
        }
        /// <summary>
        /// Gets all the partitions
        /// </summary>
        /// <returns>All the partitions</returns>
        public IEnumerable<Partition> GetPartitions()
        {
            return _partitions.Values;
        }
        /// <summary>
        /// Gets (and also updates) the parenting of a node. The disjoint set's parent will be a single partition
        /// </summary>
        /// <param name="id">Id of partition to check</param>
        /// <returns>The partition that "owns" this disjoint set</returns>
        public int GetSetParent(int id)
        {
            if (_setParenting[id] != id)
            {
                _setParenting[id] = GetSetParent(_setParenting[id]); // Go deep until we get the original node, parent of this whole disjoint set
            }
            return _setParenting[id];
        }
        /// <summary>
        /// Joins id1 and id2 partitions
        /// </summary>
        /// <param name="id1">Partition 1</param>
        /// <param name="id2">Parition 2</param>
        /// <returns>The id</returns>
        public int Join(int id1, int id2)
        {
            int firstId = _setParenting[id1];
            int secondId = _setParenting[id2];
            int minId = Math.Min(_setParenting[id1], _setParenting[id2]);
            foreach (KeyValuePair<int, int> kvp in _setParenting.Where(p => p.Value == firstId || p.Value == secondId || p.Value == minId)) // Collapse recursively, I dont care if it's not efficient im tired boss
            {
                _setParenting[kvp.Key] = minId;
            }
            return minId;
        }
    }
}
