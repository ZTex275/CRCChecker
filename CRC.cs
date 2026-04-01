using System.Diagnostics;
using System.IO;

namespace CRCChecker
{
    public class CRC
    {
        // 'order' [1..32] is the CRC polynom order, counted without the leading '1' bit
        // 'polynom' is the CRC polynom without leading '1' bit
        // 'direct' [0,1] specifies the kind of algorithm: 1=direct, no augmented zero bits
        // 'crcinit' is the initial CRC value belonging to that algorithm
        // 'crcxor' is the final XOR value
        // 'refin' [0,1] specifies if a data byte is reflected before processing (UART) or not
        // 'refout' [0,1] specifies if the CRC will be reflected before XOR
        // Data character string
        // For CRC-CCITT : order = 16, direct=1, poly=0x1021, CRCinit = 0xFFFF, crcxor=0; refin =0, refout=0  
        // For CRC16:      order = 16, direct=1, poly=0x8005, CRCinit = 0x0, crcxor=0x0; refin =1, refout=1  
        // For CRC32:      order = 32, direct=1, poly=0x4c11db7, CRCinit = 0xFFFFFFFF, crcxor=0xFFFFFFFF; refin =1, refout=1  
        // Default : CRC-CCITT

        private int order = 16;
        private ulong polynom = 0x1021;
        private int direct = 1;
        private ulong crcinit = 0xFFFF;
        private ulong crcxor = 0x0;
        private int refin = 0;
        private int refout = 0;

        private ulong crcmask;
        private ulong crchighbit;
        private ulong crcinit_direct;
        private ulong crcinit_nondirect;
        private ulong[] crctab = new ulong[256];

        public CRC(CRCCode CodingType)
        {
            switch (CodingType)
            {
                case CRCCode.CRC_CCITT:
                    order = 16; direct = 1; polynom = 0x1021; crcinit = 0xFFFF; crcxor = 0; refin = 0; refout = 0;
                    break;
                case CRCCode.CRC16:
                    order = 16; direct = 1; polynom = 0x8005; crcinit = 0x0; crcxor = 0x0; refin = 1; refout = 1;
                    break;
                case CRCCode.CRC32:
                    order = 32; direct = 1; polynom = 0x4c11db7; crcinit = 0xFFFFFFFF; crcxor = 0xFFFFFFFF; refin = 1; refout = 1;
                    break;
            }

            crcmask = ((((ulong)1 << (order - 1)) - 1) << 1) | 1;
            crchighbit = (ulong)1 << (order - 1);

            GenerateCrcTable();

            ulong bit, crc;
            int i;
            if (direct == 0)
            {
                crcinit_nondirect = crcinit;
                crc = crcinit;
                for (i = 0; i < order; i++)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if (bit != 0)
                    {
                        crc ^= polynom;
                    }
                }
                crc &= crcmask;
                crcinit_direct = crc;
            }
            else
            {
                crcinit_direct = crcinit;
                crc = crcinit;
                for (i = 0; i < order; i++)
                {
                    bit = crc & 1;
                    if (bit != 0)
                    {
                        crc ^= polynom;
                    }
                    crc >>= 1;
                    if (bit != 0)
                    {
                        crc |= crchighbit;
                    }
                }
                crcinit_nondirect = crc;
            }
        }

        public int FindCRC32(string? pathFile)
        {
            try
            {
                // Находим контрольную сумму файла sensorSettings.json
                using (Stream stream = File.Open(pathFile, FileMode.Open, FileAccess.Read))
                {
                    var rawBytes = new byte[stream.Length];
                    stream.Read(rawBytes, 0, (int)stream.Length);
                    int ulCrc32 = (int)new CRC(CRCCode.CRC32).CrcTableFast(rawBytes);
                    Debug.WriteLine($"Контрольная сумма: {ulCrc32}");
                    return ulCrc32;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("FindCRC32() -> Не удалось проверить файл sensorSettings.json!");
            }

            return 0;
        }

        public ulong CrcTableFast(byte[] p)
        {
            ulong crc = crcinit_direct;
            if (refin != 0)
            {
                crc = RefLect(crc, order);
            }
            if (refin == 0)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = (crc << 8) ^ crctab[((crc >> (order - 8)) & 0xff) ^ p[i]];
                }
            }
            else
            {
                for (int i = 0; i < p.Length; i++)
                {
                    crc = (crc >> 8) ^ crctab[(crc & 0xff) ^ p[i]];
                }
            }
            if ((refout ^ refin) != 0)
            {
                crc = RefLect(crc, order);
            }
            crc ^= crcxor;
            crc &= crcmask;
            return (crc);
        }

        private ulong RefLect(ulong crc, int bitnum)
        {
            ulong i, j = 1, crcout = 0;

            for (i = (ulong)1 << (bitnum - 1); i != 0; i >>= 1)
            {
                if ((crc & i) != 0)
                {
                    crcout |= j;
                }
                j <<= 1;
            }
            return (crcout);
        }

        private void GenerateCrcTable()
        {
            int i, j;
            ulong bit, crc;

            for (i = 0; i < 256; i++)
            {
                crc = (ulong)i;
                if (refin != 0)
                {
                    crc = RefLect(crc, 8);
                }
                crc <<= order - 8;

                for (j = 0; j < 8; j++)
                {
                    bit = crc & crchighbit;
                    crc <<= 1;
                    if (bit != 0) crc ^= polynom;
                }

                if (refin != 0)
                {
                    crc = RefLect(crc, order);
                }
                crc &= crcmask;
                crctab[i] = crc;
            }
        }
    }
}