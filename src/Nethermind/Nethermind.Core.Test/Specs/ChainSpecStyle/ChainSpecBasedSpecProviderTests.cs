﻿//  Copyright (c) 2018 Demerzel Solutions Limited
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Nethermind.Core.Specs;
using Nethermind.Serialization.Json;
using Nethermind.Specs;
using Nethermind.Specs.ChainSpecStyle;
using NUnit.Framework;

namespace Nethermind.Core.Test.Specs.ChainSpecStyle
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public class ChainSpecBasedSpecProviderTests
    {
        [Test]
        public void Rinkeby_loads_properly()
        {
            ChainSpecLoader loader = new ChainSpecLoader(new EthereumJsonSerializer());
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../../Chains/rinkeby.json");
            ChainSpec chainSpec = loader.Load(File.ReadAllText(path));
            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            RinkebySpecProvider rinkeby = RinkebySpecProvider.Instance;

            IReleaseSpec oldRinkebySpec = rinkeby.GetSpec(3660663);
            IReleaseSpec newRinkebySpec = provider.GetSpec(3660663);

            PropertyInfo[] propertyInfos = typeof(IReleaseSpec).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos.Where(pi =>
                pi.Name != "MaximumExtraDataSize"
                && pi.Name != "Registrar"
                && pi.Name != "BlockReward"
                && pi.Name != "DifficultyBombDelay"
                && pi.Name != "DifficultyBoundDivisor"))
            {
                object a = propertyInfo.GetValue(oldRinkebySpec);
                object b = propertyInfo.GetValue(newRinkebySpec);

                Assert.AreEqual(a, b, propertyInfo.Name);
            }
        }

        [Test]
        public void Mainnet_loads_properly()
        {
            ChainSpecLoader loader = new ChainSpecLoader(new EthereumJsonSerializer());
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../../Chains/foundation.json");
            ChainSpec chainSpec = loader.Load(File.ReadAllText(path));
            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            MainNetSpecProvider mainnet = MainNetSpecProvider.Instance;

            var blockNumbersToTest = new List<long>
            {
                0,
                1,
                MainNetSpecProvider.HomesteadBlockNumber - 1,
                MainNetSpecProvider.HomesteadBlockNumber,
                MainNetSpecProvider.TangerineWhistleBlockNumber - 1,
                MainNetSpecProvider.TangerineWhistleBlockNumber,
                MainNetSpecProvider.SpuriousDragonBlockNumber - 1,
                MainNetSpecProvider.SpuriousDragonBlockNumber,
                MainNetSpecProvider.ByzantiumBlockNumber - 1,
                MainNetSpecProvider.ByzantiumBlockNumber,
                MainNetSpecProvider.ConstantinopleFixBlockNumber - 1,
                MainNetSpecProvider.ConstantinopleFixBlockNumber,
                MainNetSpecProvider.IstanbulBlockNumber - 1,
                MainNetSpecProvider.IstanbulBlockNumber,
                MainNetSpecProvider.MuirGlacierBlockNumber - 1,
                MainNetSpecProvider.MuirGlacierBlockNumber,
                100000000, // far in the future
            };
            
            CompareSpecProperties(mainnet, provider, blockNumbersToTest);
            
            Assert.AreEqual(0000000, provider.GetSpec(4369999).DifficultyBombDelay);
            Assert.AreEqual(3000000, provider.GetSpec(4370000).DifficultyBombDelay);
            Assert.AreEqual(3000000, provider.GetSpec(7279999).DifficultyBombDelay);
            Assert.AreEqual(3000000, provider.GetSpec(7279999).DifficultyBombDelay);
            Assert.AreEqual(5000000, provider.GetSpec(7280000).DifficultyBombDelay);
            Assert.AreEqual(5000000, provider.GetSpec(9199999).DifficultyBombDelay);
            Assert.AreEqual(9000000, provider.GetSpec(9200000).DifficultyBombDelay);
        }

        private static void CompareSpecProperties(ISpecProvider oldSpec, ISpecProvider newSpec, IEnumerable<long> blockNumbers)
        {
            foreach (long blockNumber in blockNumbers)
            {
                PropertyInfo[] propertyInfos = typeof(IReleaseSpec).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object a = propertyInfo.GetValue(oldSpec.GetSpec(blockNumber));
                    object b = propertyInfo.GetValue(newSpec.GetSpec(blockNumber));

                    Assert.AreEqual(a, b, propertyInfo.Name);
                }
                Assert.AreEqual(oldSpec.GetSpec(blockNumber).DifficultyBombDelay, newSpec.GetSpec(blockNumber).DifficultyBombDelay);    
            }
        }

        [Test]
        public void Ropsten_loads_properly()
        {
            ChainSpecLoader loader = new ChainSpecLoader(new EthereumJsonSerializer());
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../../Chains/ropsten.json");
            ChainSpec chainSpec = loader.Load(File.ReadAllText(path));
            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            RopstenSpecProvider ropsten = RopstenSpecProvider.Instance;
            
            var blockNumbersToTest = new List<long>
            {
                0,
                1,
                RopstenSpecProvider.SpuriousDragonBlockNumber - 1,
                RopstenSpecProvider.SpuriousDragonBlockNumber,
                RopstenSpecProvider.ByzantiumBlockNumber - 1,
                RopstenSpecProvider.ByzantiumBlockNumber,
                RopstenSpecProvider.ConstantinopleFixBlockNumber - 1,
                RopstenSpecProvider.ConstantinopleFixBlockNumber,
                RopstenSpecProvider.IstanbulBlockNumber - 1,
                RopstenSpecProvider.IstanbulBlockNumber,
                RopstenSpecProvider.MuirGlacierBlockNumber - 1,
                RopstenSpecProvider.MuirGlacierBlockNumber,
                100000000, // far in the future
            };
            
            CompareSpecProperties(ropsten, provider, blockNumbersToTest);
        }

        [Test]
        public void Chain_id_is_set_correctly()
        {
            ChainSpec chainSpec = new ChainSpec {Parameters = new ChainParameters(), ChainId = 5};

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(5, provider.ChainId);
        }

        [Test]
        public void Dao_block_number_is_set_correctly()
        {
            ChainSpec chainSpec = new ChainSpec();
            chainSpec.Parameters = new ChainParameters();
            chainSpec.DaoForkBlockNumber = 23;

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(23, provider.DaoBlockNumber);
        }

        [Test]
        public void Bound_divisors_set_correctly()
        {
            ChainSpec chainSpec = new ChainSpec
            {
                Parameters = new ChainParameters {GasLimitBoundDivisor = 17},
                Ethash = new EthashParameters {DifficultyBoundDivisor = 19}
            };

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(19, provider.GenesisSpec.DifficultyBoundDivisor);
            Assert.AreEqual(17, provider.GenesisSpec.GasLimitBoundDivisor);
        }

        [Test]
        public void Difficulty_bomb_delays_loaded_correctly()
        {
            ChainSpec chainSpec = new ChainSpec
            {
                Parameters = new ChainParameters(),
                Ethash = new EthashParameters
                {
                    DifficultyBombDelays = new Dictionary<long, long>
                    {
                        {3, 100},
                        {7, 200},
                        {13, 300},
                        {17, 400},
                        {19, 500},
                    }
                }
            };

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(100, provider.GetSpec(3).DifficultyBombDelay);
            Assert.AreEqual(300, provider.GetSpec(7).DifficultyBombDelay);
            Assert.AreEqual(600, provider.GetSpec(13).DifficultyBombDelay);
            Assert.AreEqual(1000, provider.GetSpec(17).DifficultyBombDelay);
            Assert.AreEqual(1500, provider.GetSpec(19).DifficultyBombDelay);
        }

        [Test]
        public void Max_code_transition_loaded_correctly()
        {
            const long maxCodeTransition = 13;
            const long maxCodeSize = 100;

            ChainSpec chainSpec = new ChainSpec
            {
                Parameters = new ChainParameters
                {
                    MaxCodeSizeTransition = maxCodeTransition, MaxCodeSize = maxCodeSize
                }
            };

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(long.MaxValue, provider.GetSpec(maxCodeTransition - 1).MaxCodeSize, "one before");
            Assert.AreEqual(maxCodeSize, provider.GetSpec(maxCodeTransition).MaxCodeSize, "at transition");
            Assert.AreEqual(maxCodeSize, provider.GetSpec(maxCodeTransition + 1).MaxCodeSize, "one after");
        }
        
        [Test]
        public void Eip2200_is_set_correctly_directly()
        {
            ChainSpec chainSpec = new ChainSpec {Parameters = new ChainParameters {Eip2200Transition = 5}};

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            provider.GetSpec(5).IsEip2200Enabled.Should().BeTrue();
        }
        
        [Test]
        public void Eip2200_is_set_correctly_indirectly()
        {
            ChainSpec chainSpec = new ChainSpec {Parameters = new ChainParameters {Eip1706Transition = 5, Eip1283Transition = 5}};

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            provider.GetSpec(5).IsEip2200Enabled.Should().BeTrue();
        }
        
        [Test]
        public void Eip2200_is_set_correctly_indirectly_after_disabling_eip1283_and_reenabling()
        {
            ChainSpec chainSpec = new ChainSpec {Parameters = new ChainParameters {Eip1706Transition = 5, Eip1283Transition = 1, Eip1283DisableTransition = 4, Eip1283ReenableTransition = 5}};

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            provider.GetSpec(5).IsEip2200Enabled.Should().BeTrue();
        }
        
        [Test]
        public void Eip2200_is_not_set_correctly_indirectly_after_disabling_eip1283()
        {
            ChainSpec chainSpec = new ChainSpec {Parameters = new ChainParameters {Eip1706Transition = 5, Eip1283Transition = 1, Eip1283DisableTransition = 4}};

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            provider.GetSpec(5).IsEip2200Enabled.Should().BeFalse();
        }

        [Test]
        public void Eip_transitions_loaded_correctly()
        {
            const long maxCodeTransition = 1;
            const long maxCodeSize = 1;

            ChainSpec chainSpec = new ChainSpec
            {
                Ethash = new EthashParameters {HomesteadTransition = 70, Eip100bTransition = 1000},
                ByzantiumBlockNumber = 1960,
                ConstantinopleBlockNumber = 6490,
                Parameters = new ChainParameters
                {
                    MaxCodeSizeTransition = maxCodeTransition,
                    MaxCodeSize = maxCodeSize,
                    Registrar = Address.Zero,
                    MinGasLimit = 11,
                    GasLimitBoundDivisor = 13,
                    MaximumExtraDataSize = 17,
                    Eip140Transition = 1400L,
                    Eip145Transition = 1450L,
                    Eip150Transition = 1500L,
                    Eip152Transition = 1520L,
                    Eip155Transition = 1550L,
                    Eip160Transition = 1600L,
                    Eip161abcTransition = 1580L,
                    Eip161dTransition = 1580L,
                    Eip211Transition = 2110L,
                    Eip214Transition = 2140L,
                    Eip658Transition = 6580L,
                    Eip1014Transition = 10140L,
                    Eip1052Transition = 10520L,
                    Eip1108Transition = 11080L,
                    Eip1283Transition = 12830L,
                    Eip1283DisableTransition = 12831L,
                    Eip1344Transition = 13440L,
                    Eip1884Transition = 18840L,
                    Eip2028Transition = 20280L,
                    Eip2200Transition = 22000L,
                    Eip1283ReenableTransition = 23000L
                }
            };

            ChainSpecBasedSpecProvider provider = new ChainSpecBasedSpecProvider(chainSpec);
            Assert.AreEqual(long.MaxValue, provider.GetSpec(maxCodeTransition - 1).MaxCodeSize, "one before");
            Assert.AreEqual(maxCodeSize, provider.GetSpec(maxCodeTransition).MaxCodeSize, "at transition");
            Assert.AreEqual(maxCodeSize, provider.GetSpec(maxCodeTransition + 1).MaxCodeSize, "one after");

            IReleaseSpec underTest = provider.GetSpec(0L);
            Assert.AreEqual(11L, underTest.MinGasLimit);
            Assert.AreEqual(13L, underTest.GasLimitBoundDivisor);
            Assert.AreEqual(17L, underTest.MaximumExtraDataSize);

            Assert.AreEqual(long.MaxValue, underTest.MaxCodeSize);
            Assert.AreEqual(false, underTest.IsEip2Enabled);
            Assert.AreEqual(false, underTest.IsEip7Enabled);
            Assert.AreEqual(false, underTest.IsEip100Enabled);
            Assert.AreEqual(false, underTest.IsEip140Enabled);
            Assert.AreEqual(false, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(false, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1L);
            Assert.AreEqual(maxCodeSize, underTest.MaxCodeSize);
            Assert.AreEqual(false, underTest.IsEip2Enabled);
            Assert.AreEqual(false, underTest.IsEip7Enabled);
            Assert.AreEqual(false, underTest.IsEip100Enabled);
            Assert.AreEqual(false, underTest.IsEip140Enabled);
            Assert.AreEqual(false, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(70L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(false, underTest.IsEip100Enabled);
            Assert.AreEqual(false, underTest.IsEip140Enabled);
            Assert.AreEqual(false, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1000L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(false, underTest.IsEip140Enabled);
            Assert.AreEqual(false, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1400L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(false, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1450L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(false, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1500L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(false, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1520L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(false, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1550L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(false, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1580L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(false, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1600L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1700L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(false, underTest.IsEip196Enabled);
            Assert.AreEqual(false, underTest.IsEip197Enabled);
            Assert.AreEqual(false, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(false, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(1960L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(false, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(2110L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(false, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(2140L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(false, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(false, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(6580L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(false, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(10140L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(false, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(10520L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(false, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(11180L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(12830L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(true, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(12831L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(false, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(13440L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(true, underTest.IsEip1344Enabled);
            Assert.AreEqual(false, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);
            
            underTest = provider.GetSpec(18840L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(true, underTest.IsEip1344Enabled);
            Assert.AreEqual(true, underTest.IsEip1884Enabled);
            Assert.AreEqual(false, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(20280L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(true, underTest.IsEip1344Enabled);
            Assert.AreEqual(true, underTest.IsEip1884Enabled);
            Assert.AreEqual(true, underTest.IsEip2028Enabled);
            Assert.AreEqual(false, underTest.IsEip2200Enabled);

            underTest = provider.GetSpec(22000L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(false, underTest.IsEip1283Enabled);
            Assert.AreEqual(true, underTest.IsEip1344Enabled);
            Assert.AreEqual(true, underTest.IsEip1884Enabled);
            Assert.AreEqual(true, underTest.IsEip2028Enabled);
            Assert.AreEqual(true, underTest.IsEip2200Enabled);
            
            underTest = provider.GetSpec(23000L);
            Assert.AreEqual(underTest.MaxCodeSize, maxCodeSize);
            Assert.AreEqual(true, underTest.IsEip2Enabled);
            Assert.AreEqual(true, underTest.IsEip7Enabled);
            Assert.AreEqual(true, underTest.IsEip100Enabled);
            Assert.AreEqual(true, underTest.IsEip140Enabled);
            Assert.AreEqual(true, underTest.IsEip145Enabled);
            Assert.AreEqual(true, underTest.IsEip150Enabled);
            Assert.AreEqual(true, underTest.IsEip152Enabled);
            Assert.AreEqual(true, underTest.IsEip155Enabled);
            Assert.AreEqual(true, underTest.IsEip158Enabled);
            Assert.AreEqual(true, underTest.IsEip160Enabled);
            Assert.AreEqual(true, underTest.IsEip170Enabled);
            Assert.AreEqual(true, underTest.IsEip196Enabled);
            Assert.AreEqual(true, underTest.IsEip197Enabled);
            Assert.AreEqual(true, underTest.IsEip198Enabled);
            Assert.AreEqual(true, underTest.IsEip211Enabled);
            Assert.AreEqual(true, underTest.IsEip214Enabled);
            Assert.AreEqual(true, underTest.IsEip649Enabled);
            Assert.AreEqual(true, underTest.IsEip658Enabled);
            Assert.AreEqual(true, underTest.IsEip1014Enabled);
            Assert.AreEqual(true, underTest.IsEip1052Enabled);
            Assert.AreEqual(true, underTest.IsEip1108Enabled);
            Assert.AreEqual(true, underTest.IsEip1234Enabled);
            Assert.AreEqual(true, underTest.IsEip1283Enabled);
            Assert.AreEqual(true, underTest.IsEip1344Enabled);
            Assert.AreEqual(true, underTest.IsEip1884Enabled);
            Assert.AreEqual(true, underTest.IsEip2028Enabled);
            Assert.AreEqual(true, underTest.IsEip2200Enabled);
        }
    }
}