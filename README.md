
# MerkleTools

MerkleTools-solidity is a .NET library to create and validate Merkle trees and receipts compatible with @openzeppelin/contracts/utils/cryptography/MerkleProof.sol using keccak256 hash algorithm.


## How to use?
With nuget :
> **Install-Package MerkleTools-solidity** 

Go on the [nuget website](https://www.nuget.org/packages/MerkleTools-solidity/) for more information.

## Example

```c#
using MerkleTools;
using System.Text;

MerkleTree tree = new MerkleTree();

string[] addresses = {  "0xa18FBe02d2f247922a8e7A9B3962cfd3b8aEE0Ca", 
                        "0x5B38Da6a701c568545dCfcB03FcB875f56beddC4", 
                        "0x25f7fF7917555132eDD3294626D105eA1C797250", 
                        "0xF6574D878f99D94896Da75B6762fc935F34C1300"};

for (int i = 0; i < addresses.Length; i++)
{
    byte[] temp = Encoding.ASCII.GetBytes(addresses[i]);
    tree.AddLeaf(temp, true);
}

var root = Encoding.UTF8.GetString(tree.MerkleRootHash);
Console.WriteLine(root);

//0x2bd66355c24f675a8a9411593198b3aacaec256d58a2ce950c798b7e9a378f28


var proof = tree.GetProof(0);
Console.WriteLine(proof.ToJson());

/*[ "0x5931b4ed56ace4c46b68524cb5bcbf4195f1bbaacbe5228fbd090546c88dd229",
    "0x1f957db768cd7253fad82a8a30755840d536fb0ffca7c5c73fe9d815b1bc2f2f"]*/


var address = Encoding.ASCII.GetBytes(addresses[0]);
var addressHash = MerkleTree.keccakFromByte(address);
var isValid = tree.ValidateProof(proof, addressHash);
Console.WriteLine(isValid); //true
```

## Contact

Contact [beneyim@gmail.com](mailto:beneyim@gmail.com) with questions