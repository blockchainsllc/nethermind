//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Encoding;
using Nethermind.Core.Specs;
using Nethermind.Logging;
using Nethermind.Store;

namespace Nethermind.Blockchain.Receipts
{
    public class PersistentReceiptStorage : IReceiptStorage
    {
        private readonly IDb _database;
        private readonly ISpecProvider _specProvider;
        private readonly ILogger _logger;

        public PersistentReceiptStorage(IDb receiptsDb, ISpecProvider specProvider, ILogManager logManager)
        {
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            _database = receiptsDb ?? throw new ArgumentNullException(nameof(receiptsDb));
            _specProvider = specProvider ?? throw new ArgumentNullException(nameof(specProvider));

            byte[] lowestBytes = _database.Get(Keccak.Zero);
            LowestInsertedReceiptBlock = lowestBytes == null ? (long?) null : new RlpStream(lowestBytes).DecodeLong();
        }

        public TxReceipt Find(Keccak hash)
        {
            var receiptData = _database.Get(hash);
            if (receiptData != null)
            {
                var receipt = Rlp.Decode<TxReceipt>(new Rlp(receiptData), RlpBehaviors.Storage);
                receipt.TxHash = hash;
                return receipt;
            }

            return null;
        }

        public void Add(TxReceipt txReceipt, bool isProcessed)
        {
            if (txReceipt == null)
            {
                throw new ArgumentNullException(nameof(txReceipt));
            }

            var spec = _specProvider.GetSpec(txReceipt.BlockNumber);
            RlpBehaviors behaviors = spec.IsEip658Enabled ? RlpBehaviors.Eip658Receipts : RlpBehaviors.None;
            if (isProcessed)
            {
                behaviors = behaviors | RlpBehaviors.Storage;
            }

            _database.Set(txReceipt.TxHash,
                Rlp.Encode(txReceipt, behaviors).Bytes);
        }
        
        public void Insert(long blockNumber, TxReceipt txReceipt)
        {
            if (txReceipt == null && blockNumber != 1L)
            {
                throw new ArgumentNullException(nameof(txReceipt));
            }
            
            if (txReceipt != null)
            {
                var spec = _specProvider.GetSpec(blockNumber);
                RlpBehaviors behaviors = spec.IsEip658Enabled ? RlpBehaviors.Eip658Receipts : RlpBehaviors.None;
                _database.Set(txReceipt.TxHash, Rlp.Encode(txReceipt, behaviors).Bytes);
            }

            UpdateLowestInsertedInDb(blockNumber);
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Insert(List<(long BlockNumber, TxReceipt TxReceipt)> receipts)
        {
            long? lowestSoFar = LowestInsertedReceiptBlock;
            Guid batchGuid = Guid.NewGuid();
            _database.StartBatch(batchGuid);
            try
            {
                for (int i = 0; i < receipts.Count; i++)
                {
                    TxReceipt txReceipt = receipts[i].TxReceipt;
                    long blockNumber = receipts[i].BlockNumber;
                    if (txReceipt == null && blockNumber != 1L)
                    {
                        throw new ArgumentNullException(nameof(txReceipt));
                    }

                    if (txReceipt != null)
                    {
                        var spec = _specProvider.GetSpec(blockNumber);
                        RlpBehaviors behaviors = spec.IsEip658Enabled ? RlpBehaviors.Eip658Receipts : RlpBehaviors.None;
                        _database.Set(txReceipt.TxHash, Rlp.Encode(txReceipt, behaviors).Bytes);
                    }

                    lowestSoFar =  Math.Min(LowestInsertedReceiptBlock ?? long.MaxValue, blockNumber);
                }
                
                UpdateLowestInsertedInDb(lowestSoFar);
            }
            finally
            {
                _database.CommitBatch(batchGuid);
            }
        }

        private void UpdateLowestInsertedInDb(long? lowestSoFar)
        {
            LowestInsertedReceiptBlock = Math.Min(LowestInsertedReceiptBlock ?? long.MaxValue, lowestSoFar ?? long.MaxValue);
//              LowestInsertedReceiptBlock = Math.Min(LowestInsertedReceiptBlock ?? long.MaxValue, blockNumber);
            _database.Set(Keccak.Zero, Rlp.Encode(LowestInsertedReceiptBlock.Value).Bytes);
        }

        public long? LowestInsertedReceiptBlock { get; private set; }
    }
}