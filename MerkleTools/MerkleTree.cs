using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace MerkleTools
{
	public class MerkleTree
	{
		private readonly List<MerkleLeaf> _leave;
		private MerkleNodeBase _root;
		private bool _recalculate;

		public byte[] MerkleRootHash => Root?.Hash;

		internal MerkleNodeBase Root
		{
			get
			{
				if (_recalculate)
				{
					_root = MerkleNode.Build(_leave);
					_recalculate = false;
				}
				return _root;
			}
		}



		public MerkleTree()
		{
			_leave = new List<MerkleLeaf>();
		}

		public void AddLeaf(byte[] data, bool mustHash=false)
		{
			var hash = mustHash ? keccakFromByte(data) : data;
			_leave.Add(new MerkleLeaf(hash));
			_recalculate = true;
		}

		public void AddLeave(IEnumerable<byte[]> items, bool mustHash = false)
		{
			foreach (var item in items)
			{
				AddLeaf(item, mustHash);
			}
		}

		internal Proof GetProof(MerkleLeaf leaf)
		{
			var proof = new Proof(leaf.Hash, Root.Hash);
			var node = (MerkleNodeBase)leaf;
			while (node.Parent !=null)
			{
				if (node.Parent.Left == node)
				{
					proof.AddRight(node.Parent.Right.Hash);
				}
				else
				{
					proof.AddLeft(node.Parent.Left.Hash);
				}
				node = node.Parent;
			}
			return proof;
		}

		public Proof GetProof(int index)
		{
			return GetProof(_leave[index]);
		}

		public Proof GetProof(byte[] hash)
		{
			try
			{
				var leaf = _leave.Single(x => x.Hash.SequenceEqual(hash));
				return GetProof(leaf);
			}
			catch (InvalidOperationException e)
			{
				throw new InvalidOperationException("There is not single hash matching", e);
			}
		}

		public bool ValidateProof(Proof proof, byte[] hash)
		{
			return proof.Validate(hash, MerkleRootHash);
		}

		public int Levels => Root.Level;

		public static byte[] Melt(byte[] h1, byte[] h2)
		{
			var buffer = new byte[h1.Length + h2.Length];
			Buffer.BlockCopy(h1, 0, buffer, 0, h1.Length);
			Buffer.BlockCopy(h2, 0, buffer, h1.Length, h2.Length);
			return keccakFromByte(buffer);
		}


		public static byte[] keccakFromTwoByte(byte[] _data, byte[] _data2)
		{
			var rawTransaction = System.Text.Encoding.UTF8.GetString(_data);
			var rawTransaction2 = System.Text.Encoding.UTF8.GetString(_data2);

			int offset = rawTransaction.StartsWith("0x") ? 2 : 0;
			int offset2 = rawTransaction2.StartsWith("0x") ? 2 : 0;
			var txByte1 = Enumerable.Range(offset, rawTransaction.Length - offset)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(rawTransaction.Substring(x, 2), 16))
							 .ToArray();

			var txByte2 = Enumerable.Range(offset2, rawTransaction2.Length - offset2)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(rawTransaction2.Substring(x, 2), 16))
							 .ToArray();


			byte[] txByte = txByte1.Concat(txByte2).ToArray();


			//Note: Not intended for intensive use so we create a new Digest.
			//if digest reuse, prevent concurrent access + call Reset before BlockUpdate
			var digest = new KeccakDigest(256);

			digest.BlockUpdate(txByte, 0, txByte.Length);
			var calculatedHash = new byte[digest.GetByteLength()];
			digest.DoFinal(calculatedHash, 0);
			var transactionHash = BitConverter.ToString(calculatedHash, 0, 32).Replace("-", "").ToLower();
			var temp = "0x" + transactionHash;


			return Encoding.ASCII.GetBytes((temp));
		}


		public static byte[] keccakFromByte(byte[] _data)
		{
			if (_data == null) return null;
			var rawTransaction = System.Text.Encoding.UTF8.GetString(_data);


			int offset = rawTransaction.StartsWith("0x") ? 2 : 0;
			var txByte = Enumerable.Range(offset, rawTransaction.Length - offset)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(rawTransaction.Substring(x, 2), 16))
							 .ToArray();

			var digest = new KeccakDigest(256);

			digest.BlockUpdate(txByte, 0, txByte.Length);
			var calculatedHash = new byte[digest.GetByteLength()];
			digest.DoFinal(calculatedHash, 0);

			var transactionHash = BitConverter.ToString(calculatedHash, 0, 32).Replace("-", "").ToLower();
			var temp = "0x" + transactionHash;


			return Encoding.ASCII.GetBytes((temp));
		}




	}
}