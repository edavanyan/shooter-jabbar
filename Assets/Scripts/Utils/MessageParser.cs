using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

public class MessageParser
{
    private string data = "\n{id : \"adsefsdf\",message : \"map\",data : {players : {qaq:{id:\"adadsd\",position:{x:23, y:23},health:10,color: \"RED\"},qaq:{id:\"adadsd\",position:{x:23, y:23},health:10,color: \"RED\"}},aids : {qaq:{positon : {x:23, y:23}},qaqo:{positon : {x:23, y:23}}}}}";
    public Dictionary<string, object> ParseMessage(string message)
    {
        var json = JsonConvert.DeserializeObject<JObject> (message);
        var dictionary = ParseDict(json.ToString());
        return dictionary;
    }

    private Dictionary<string, object> ParseDict(string json)
    {
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        var keys = keysPool.NewItem();
        foreach (var (key, value) in dictionary)
        {
            keys.Add(key);
        }

        foreach (string key in keys)
        {
            if (dictionary[key] is JObject)
            {
                dictionary[key] = ParseDict(dictionary[key].ToString());
            }
        }
        keysPool.DestoryItem(keys);

        return dictionary;
    }

    private ObjectPool<KeyCollection> keysPool = new ObjectPool<KeyCollection>(new KeyCollection());

    private struct KeyCollection : IPoolable, IEnumerable<string>
    {
        private Queue<string> keys;
        
        public void Add(string key)
        {
            keys.Enqueue(key);
        }
        
        public void New()
        {
            if (keys == null)
            {
                keys = new Queue<string>();
            }
        }

        public void Free()
        {
            keys.Clear();
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return keys.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return keys.GetEnumerator();
        }
    }
}
