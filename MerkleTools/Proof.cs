using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MerkleTools
{
    public class Proof : IEnumerable<ProofItem>
	{
		public byte[] Target { get; }
		public byte[] MerkleRoot { get; }
		private readonly List<ProofItem> _proofItems = new List<ProofItem>();

		public ProofItem this[int i] => _proofItems[i];

		public Proof(byte[] target, byte[] merkleRoot)
		{
			Target = target;
			MerkleRoot = merkleRoot;
		}

		public void AddLeft(byte[] hash)
		{
			_proofItems.Add(new ProofItem(Branch.Left, hash));
		}
		public void AddRight(byte[] hash)
		{
			_proofItems.Add(new ProofItem(Branch.Rigth, hash));
		}

		public bool Validate()
		{
			return Validate(Target, MerkleRoot);
		}

		public bool Validate(byte[] hash, byte[] root)
		{
			var proofHash = hash;
			foreach (var x in this)
			{
				if (MerkleNode.IsSmallByte(proofHash, x.Hash))
				{
					proofHash = MerkleTree.keccakFromTwoByte(proofHash, x.Hash);
				}
				else
				{
					proofHash = MerkleTree.keccakFromTwoByte(x.Hash, proofHash);
				}
			}
			return proofHash.SequenceEqual(root);
		}

		public IEnumerator<ProofItem> GetEnumerator()
		{
			return _proofItems.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Receipt ToReceipt()
		{
			return new Receipt(this);
		}

		public string ToJson()
		{
			return $"[{string.Join(",", this.Select(x=>x.ToJson()))}]";
		}
	}
}