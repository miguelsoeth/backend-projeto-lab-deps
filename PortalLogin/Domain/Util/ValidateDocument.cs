namespace Domain.Util;
using System.Linq;
using System.Text.RegularExpressions;

public class ValidateDocument
{
    public static bool CpfCnpj(string documento)
    {
        if (documento.Length == 11)
        {
            return Cpf(documento);
        }
        else if (documento.Length == 14)
        {
            return Cnpj(documento);
        }
        return false;
    }

    public static bool Cpf(string cpf)
    {
        cpf = Regex.Replace(cpf, "[^0-9]", "");
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        int sum = 0, remainder;
        for (int i = 1; i <= 9; i++)
        {
            sum += int.Parse(cpf.Substring(i - 1, 1)) * (11 - i);
        }

        remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11)
        {
            remainder = 0;
        }
        if (remainder != int.Parse(cpf.Substring(9, 1)))
        {
            return false;
        }

        sum = 0;
        for (int i = 1; i <= 10; i++)
        {
            sum += int.Parse(cpf.Substring(i - 1, 1)) * (12 - i);
        }

        remainder = (sum * 10) % 11;
        if (remainder == 10 || remainder == 11)
        {
            remainder = 0;
        }
        return remainder == int.Parse(cpf.Substring(10, 1));
    }

    public static bool Cnpj(string cnpj)
    {
        cnpj = Regex.Replace(cnpj, "[^0-9]", "");
        if (cnpj.Length != 14)
        {
            return false;
        }

        int length = cnpj.Length - 2;
        string numbers = cnpj.Substring(0, length);
        string digits = cnpj.Substring(length);
        int sum = 0;
        int pos = length - 7;

        for (int i = length; i >= 1; i--)
        {
            sum += int.Parse(numbers[length - i].ToString()) * pos--;
            if (pos < 2)
            {
                pos = 9;
            }
        }

        int result = sum % 11 < 2 ? 0 : 11 - (sum % 11);
        if (result != int.Parse(digits[0].ToString()))
        {
            return false;
        }

        length += 1;
        numbers = cnpj.Substring(0, length);
        sum = 0;
        pos = length - 7;

        for (int i = length; i >= 1; i--)
        {
            sum += int.Parse(numbers[length - i].ToString()) * pos--;
            if (pos < 2)
            {
                pos = 9;
            }
        }

        result = sum % 11 < 2 ? 0 : 11 - (sum % 11);
        return result == int.Parse(digits[1].ToString());
    }
}