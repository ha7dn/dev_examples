// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Random res = new Random();

// String that contain both alphabets and numbers
String str = "abcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()+-=";
int size = 8;

// Initializing the empty string
String randomstring = "";

for (int i = 0; i < size; i++)
{

    // Selecting a index randomly
    int x = res.Next(str.Length);

    // Appending the character at the 
    // index to the random alphanumeric string.
    randomstring = randomstring + str[x];
}

Console.WriteLine("Random alphanumeric String:" + randomstring);

Console.WriteLine("Fin Proceso");
