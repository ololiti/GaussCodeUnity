using System.Collections;
using System.Collections.Generic; 
using UnityEngine;


/*
 * Aditi Talati - 28 Aug 2018
 * inputs one possible Gauss code of a knot or link shadow
 * and the IDs of crossings that are marked (their sign is known)
 * 
 * outputs a list of possible Gauss codes for the shadow
 */

public class ClosedCurve : MonoBehaviour
{

    private string stringCode; //stores the Gauss code
    private Info[] usefulCode; //stores the code for ease of access
    private bool[,] orientations; //possible crossing orientation combos
                                  // true if +, false if -
                                  // Use this for initialization
    private int[] switchable; //the crossing IDs that will be switched
    private int orientationsX; //the x length of orientations
    private int orientationsY; //the y length of orientations



    public ClosedCurve (string gauss, string marked) {
        stringCode = gauss;
        int numBreaks = 0; //find the number of breaks between links in order to
                           //separate the data into a readable form correctly
        int linkIndex = stringCode.IndexOf('|');
        while (linkIndex != -1)
        {
            numBreaks++;
            linkIndex = stringCode.IndexOf('|', linkIndex + 1);
        }

        //store the information in a readable format
        int length = 0;
        if (stringCode.Length - numBreaks <= 54)
            length = (stringCode.Length - numBreaks) / 3;
        else if (stringCode.Length - numBreaks <= 594)
            length = 18 + (stringCode.Length - numBreaks - 54) / 4;
        usefulCode = new Info[length];
        //System.out.println("length: " + length);
        int i = 0;
        int index = 0;
        while (i < stringCode.Length)
        {
            bool above;
            int num;
            bool end;
            bool right;

            //set above value
            if (stringCode.Substring(i,1).Equals("a")) above = true;
            else if (stringCode.Substring(i,1).Equals("b")) above = false;
            else
            {
                Debug.Log(stringCode.Substring(i,1) + " is not a or b");
                above = false;
            }
            i++;

            //set crossing id number
            num = System.Int32.Parse(stringCode.Substring(i, 1));
            i++;
            int unitsDigit;
            if (System.Int32.TryParse(stringCode.Substring(i, 1), out unitsDigit)){
                num *= 10;
                num += unitsDigit;
                i++;
                //System.out.println(num);
            }

            //set "right" value (true if positive crossing, false if negative)
            if (stringCode.Substring(i,1).Equals("+")) right = true;
            else if (stringCode.Substring(i,1).Equals("-")) right = false;
            else
            {
                Debug.Log(stringCode.Substring(i,1) + " is not + or -");
                right = false;
            }
            i++;

            //check if it's the end of a link
            if (i < stringCode.Length && stringCode.Substring(i,1).Equals("|"))
            {
                end = true;
                i++;
            }
            else
            {
                end = false;
            }

            //save as Info object
            usefulCode[index] = new Info(above, num, end, right);
            index++;
            //System.out.println(index-1 + ": " + usefulCode[index-1]);
        }

        //create the array of crossings to be switched
        string[] markedArray = marked.Split(' ');
        int[] set = new int[markedArray.Length];
        for (int j = 0; j < set.Length; j++){
            set[j] = System.Int32.Parse(markedArray[j]);
        }
        switchable = new int[length / 2 - set.Length];
        int setIndex = 0;
        int switchableIndex = 0;
        for (int j = 1; j <= length / 2; j++)
        {
            if (setIndex < set.Length && j == set[setIndex])
                setIndex++;
            else
            {
                switchable[switchableIndex] = j;
                switchableIndex++;
            }
        }

        //create the array of possible orientations
        //each row is one closed curve with each term in the row being
        //a crossing
        orientationsX = (int)System.Math.Pow(2, switchable.Length);
        orientationsY = usefulCode.Length / 2;
        orientations = new bool[orientationsX, orientationsY];
        for (int j = 0; j < orientationsY; j++)
        {
            orientations[0,j] = crossing(j + 1);
        }

        testPossibilities(0, orientationsX, 0, 0);
    }

    //lists out the Gauss codes of all possible closed curves
    public void testPossibilities(int beginning, int end, int switchableIndex,
                                  int prevIndex)
    {
        int currentIndex = (beginning + end) / 2;
        if (currentIndex >= orientationsX) 
            Debug.LogError("current index error");
        if (switchableIndex < switchable.Length)
        {
            //copies all the values from the previous one
            for (int i = 0; i < orientationsY; i++){
                orientations[currentIndex, i] = orientations[prevIndex, i];
            }
            //flips the crossing for one position
            orientations[currentIndex, switchable[switchableIndex] - 1] =
                    !orientations[prevIndex, switchable[switchableIndex] - 1];

            testPossibilities(beginning, currentIndex, switchableIndex + 1,
                                                       beginning);
            testPossibilities(currentIndex, end, switchableIndex + 1,
                                                 currentIndex);

        }
    }

    private string[] writtenCodes()
    {
        string[] writtenCodes = new string[(int)System.Math.Pow(2, switchable.Length)];
        for (int i = 0; i < writtenCodes.Length; i++)
        {
            writtenCodes[i] = "";
            for (int j = 0; j < usefulCode.Length; j++)
            {
                usefulCode[j].swapOrientation(orientations[i, usefulCode[j].num - 1]);
                writtenCodes[i] += usefulCode[j];
            }
        }
        return writtenCodes;
    }

    //looks for a crossing and tells you whether it's positive
    private bool crossing(int id)
    {
        foreach (Info i in usefulCode)
        {
            if (i.num == id) return i.right;
        }
        //System.out.println("no crossing found with id " + id);
        return false;
    }

    public string toString()
    {
        string[] output = writtenCodes();
        string outputString = "";
        foreach (string s in output)
        {
            outputString += s + "\n";
        }
        return outputString;
    } 


    // Update is called once per frame
    void Update () {
		
	}

    class Info{ //stores the information about each term in the Gauss code 
        bool above; //true if gauss code for that term is "a"
        public int num; //the id number of the crossing 
        bool end; //true if the term is right before a link separator
        public bool right; //true if it is a right hand (+) crossing
        public Info(bool above, int num, bool end, bool right){
            this.above = above;
            this.num = num;
            this.end = end;
            this.right = right;
        }
        public void swapOrientation(bool right)
        {
            //switches orientation if it needs to
            if (this.right != right)
            {
                this.right = right;
                above = !above;
            }
        }
        public override string ToString(){
            string output = "";

            if (above) output += "a";
            else output += "b";

            output += num;

            if (right) output += "+";
            else output += "-";

            if (end) output += "|";

            return output;
        }
    }

}
