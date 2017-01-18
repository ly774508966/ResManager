using UnityEngine;
using System.IO;
using System.Collections;

public class GameEncoder
{
    public static bool EncodeBytes(ref byte[] data)
    {
        return EncodeBytes(ref data, 0, data.Length, "WLGame", 2014);
    }

    public static bool EncodeBytes(ref byte[] data, int index, long length, string strKey, int nKey)
    {
        if (data == null || strKey == null || index >= length)
        {
            Debuger.LogError("EncodeBytes Error : Invalid argument !!!");
            return false;
        }

        try
        {
            int keySumValue = 0;
            for (int i = 0; i < strKey.Length; i++)
            {
                keySumValue += strKey[i];
            }

            long l = index; int k = 0; long r = length;
            for (; l < index + length; l++, k++, r--)
            {
                if (k >= strKey.Length)
                    k = 0;
                data[l] = (byte)(data[l] + r + nKey);
                data[l] = (byte)(data[l] ^ strKey[k]);
                data[l] = (byte)(data[l] ^ keySumValue);
            }
        }
        catch (System.Exception e)
        {
            Debuger.LogError("EncodeBytes Error : " + e.Message);
            return false;
        }

        return true;
    }

    public static bool DecodeBytes(ref byte[] data)
    {
        return DecodeBytes(ref data, 0, data.Length, "WLGame", 2014);
    }

    public static bool DecodeBytes(ref byte[] data, int index, long length, string strKey, int nKey)
    {
        if (data == null || strKey == null || index >= length)
        {
            Debuger.LogError("DecodeBytes Error : Invalid argument !!!");
            return false;
        }

        try
        {
            int keySumValue = 0;
            for (int i = 0; i < strKey.Length; i++)
            {
                keySumValue += strKey[i];
            }

            long l = index; int k = 0; long r = length;
            for (; l < index + length; l++, k++, r--)
            {
                if (k >= strKey.Length)
                    k = 0;
                data[l] = (byte)(data[l] ^ keySumValue);
                data[l] = (byte)(data[l] ^ strKey[k]);
                data[l] = (byte)(data[l] - r - nKey);
            }
        }
        catch (System.Exception e)
        {
            Debuger.LogError("DecodeBytes Error : " + e.Message);
            return false;
        }

        return true;
    }
}