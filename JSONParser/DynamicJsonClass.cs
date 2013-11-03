using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace JSONParser
{
    struct JSONKEYWORDS
    {
        public readonly char listOpener;
        public readonly char objectOpener;
        public readonly char stringStart;
        public readonly char valueStart;
        public readonly char listCloser;
        public readonly char objectCloser;
        public readonly char stringEnd;

        public JSONKEYWORDS(bool doNothing)
        {
            listOpener = '[';
            objectOpener = '{';
            stringStart = '"';
            valueStart = ':';
            listCloser = ']';
            objectCloser = '}';
            stringEnd = '"';
        }
    }

    class DynamicJsonClass
    {
        ExpandoObject baseClass = new ExpandoObject();
        Stack<char> keywords = new Stack<char>();
        StringBuilder jsonText = null;
        JSONKEYWORDS possibleTokens = new JSONKEYWORDS(true);
        Dictionary<char, char> keywordsEq = null;
        int index = 0;

        public DynamicJsonClass()
        {
            keywordsEq = new Dictionary<char, char>();
            keywordsEq[possibleTokens.objectOpener] = possibleTokens.objectCloser;
            keywordsEq[possibleTokens.listOpener] = possibleTokens.listCloser;
            keywordsEq[possibleTokens.stringStart] = possibleTokens.stringEnd;
        }

        public dynamic JSONDESerialize(string json)
        {
            jsonText = new StringBuilder(json);
            char token = ReadNextCharacter();
            if (token == possibleTokens.objectOpener)
               baseClass = ParseObject();
            else
                throw new JsonBadFormattedException(index, "I was expecting object opener, {");
            
            return baseClass;
        }

        dynamic ParseObject()
        {
          /*  char token = (char)jsonText[index];
            if (token != possibleTokens.objectOpener)
            {
                throw new JsonBadFormattedException(index);
            }

            index++;*/
            ExpandoObject myBaseObject = new ExpandoObject();
            char token = possibleTokens.objectOpener;
            keywords.Push(token);

            while (index < ( jsonText.Length - 1 ) && token != possibleTokens.objectCloser )
            {
                token = (char)jsonText[index];
                index++;
                switch (token)
                {
                    case ' ':
                        break;

                    case '"':
                        NewProperty(myBaseObject);
                        break;

                    case '}':
                        if (keywords.Pop() != possibleTokens.objectOpener)
                            throw new JsonBadFormattedException(index, "End of object, but not start of object");
                        break;

                    case '{':
                        keywords.Push(possibleTokens.objectOpener);
                        ParseObject();
                        break;

                    default: break;
                       // throw new JsonBadFormattedException(index, "I was waiting for one token");
                }
            }
            return myBaseObject;
        }

        void NewProperty(dynamic myBaseClass)
        {
            IDictionary<string, object> objectBase = myBaseClass as IDictionary<string, object>;
            List<object> myL = null;
            string valueString = string.Empty;
            float valueFloat = 0;
            string propertyName = ReadProperty();
            char car = ReadNextCharacter();

            if (car != ':')
                throw new JsonBadFormattedException(index, "I miss \":\"");

            //tras volver de ReadNextCharacter, index apunta al siguiente caracter
            car = ReadNextCharacter();

            //Puede ser un numero la propiedad
            if (Char.IsDigit(car))
            {
                valueFloat = ReadFloatValue(car);
                objectBase.Add(propertyName, valueFloat);
                return;
            }

            //la propiedad es un string
            else if (car == possibleTokens.stringStart)
            {
                valueString = ReadProperty();
                objectBase.Add(propertyName, valueString);
                return;
            }

            else if (car == possibleTokens.listOpener)
            {
                myL = ReadList();
                objectBase.Add(propertyName, myL);
            }

            else if (car == possibleTokens.objectOpener)
            {
                dynamic myDynamicObject = ParseObject();
                objectBase.Add(propertyName, myDynamicObject);
            }
        }

        List<object> ReadList()
        {
            char car = ReadNextCharacter();
            List<object> myL = new List<object>();
            keywords.Push(possibleTokens.listOpener);

            while (car != possibleTokens.listCloser)
            {
                if (Char.IsDigit(car))
                {
                    myL.Add(ReadFloatValue(car));
                }

                else if (car == possibleTokens.stringStart)
                {
                    myL.Add(ReadProperty());
                }

                else if (car == possibleTokens.objectOpener)
                {
                    myL.Add(ParseObject());
                }

                if (car == possibleTokens.listOpener)
                {
                    myL.Add(ReadList());
                }
                car = jsonText[index];
                index++;
            }

            if (keywords.Pop() != possibleTokens.listOpener)
                throw new JsonBadFormattedException(index, "I expected ]");

            return myL;
        }

        float ReadFloatValue(char firstDigit)
        {
            StringBuilder number = new StringBuilder();
            number.Append(firstDigit);

            char car = jsonText[index];

            while (Char.IsDigit(car) || car == '.')
            {
                number.Append(car);
                index++;
                car = jsonText[index];
            }

            return float.Parse(number.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        }

        string ReadProperty()
        {
            StringBuilder newString = new StringBuilder();
            char car = jsonText[index];

            while (car != '"')
            {
                index++;
                newString.Append(car);
                car = jsonText[index];
            }

            if (string.IsNullOrWhiteSpace(newString.ToString()))
                throw new JsonBadFormattedException(index, "Property null or only white spaces");

            //index apunta al siguiente caracter
            index++;
            return newString.ToString();
        }

        char ReadNextCharacter()
        {
            char car = jsonText[index];
            
            while (car == ' ')
            {
                index++;
                car = jsonText[index];
            }

            index++;
            return car;
        }
    }
}
