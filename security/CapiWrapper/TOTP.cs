using System;

namespace mb_integrator_hub.src.mb_integrations.mb_security_functions.business_logic.historical_implementation.CapiWrapper
{
    public class TotpImpl
    {
        /// <summary>
        /// секретный ключ
        /// </summary>
        byte[] m_key = null;

        /// <summary>
        /// Приводит long к byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private byte[] ConverLongToByte(long value)
        { 
            byte[] result = new byte[8];

            for (int i = 0; i < 8; ++i)
            {
                result[i] = (byte)(value >> (8 - i - 1 << 3));
            }

            return result;
        }

        /// <summary>
        /// Обрезает хеш до 4 байт
        /// </summary>
        /// <param name="value"></param>
        private uint CutValue(byte[] value)
        {
            if (value == null || value.Length < 20)
            {
                throw new SystemException("TOTP ERROR! Hash value inorrect.");
            }

            byte offset = (byte)(value[19] & 0x0f);
            
            uint dbc = (uint)((value[offset] & 0x7f) << 24 |
                                   (value[offset + 1] & 0xff) << 16 |
                                   (value[offset + 2] & 0xff) << 8 |
                                   (value[offset + 3] & 0xff));
            
            dbc %= 100000000;

            return dbc;
        }

        /// <summary>
        /// Вычисляет HMAC
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private byte[] CreateHash(byte[] data, byte[] key)
        {
            if (data == null)
            {
                throw new SystemException("TOTP Error! Hashed data is null.");
            }

            if (key == null)
            {
                throw new SystemException("TOTP Error! Key is null.");
            }

            Hmac hmac = new Hmac();
            hmac.SetKey(key);
            hmac.Add(data);

            byte[] hash = hmac.GetValue();
            return hash;
        }
        
        /// <summary>
        /// Создает знчение TOTP счетчика
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public uint CreateValue(long unixTime)
        {
            byte[] time = ConverLongToByte(unixTime);
            byte[] hash = CreateHash(time, m_key);
            
            uint dbc = CutValue(hash);
            return dbc;
        }

        public void SetKey(byte[] key)
        {
            m_key = key ?? throw new SystemException("TOTP Error! Set key error! Key is null.");
        }
    }

    public class TOTP
    {

        /// <summary>
        /// 
        /// </summary>
        byte[] m_userKey = null;

        /// <summary>
        /// 
        /// </summary>
        const int TIME_CHANK = 30;

        const int SEARCH_WINDOW = 50;

        /// <summary>
        /// 
        /// </summary>
        readonly TotpImpl m_generator = new TotpImpl();

        public TOTP()
        {
        }

        public bool InitKey(byte[] wrapKey)
        {
            m_userKey = DecryptKey(wrapKey);
            m_generator.SetKey(m_userKey);
            return true;
        }

        public byte[] DecryptKey(byte[] encryptedValue)
        {
            // todo: decrypt
            return encryptedValue;
        }

        private long GetUnixTime(DateTime tstmp)
        {
            var timeSpan = (tstmp - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        public bool CheckTotp(uint totp, ref long newTimeStamp)
        {
            DateTime currentTime = DateTime.UtcNow;
            int startOffset = -1 * (GetValidInterval() * SEARCH_WINDOW); 
            DateTime offsetTime = currentTime.AddSeconds(startOffset);
            long timeCode = GetUnixTime(offsetTime) / GetValidInterval();
            
            
            for (int i = 0; i < 100; ++i)
            {
                uint code = m_generator.CreateValue(timeCode++);
                
                if (code == totp)
                {
                    newTimeStamp = timeCode;
                    return true;
                }
            }

            return false;
        }

        private int GetValidInterval()
        {
            return TIME_CHANK;
        }
    }
}
