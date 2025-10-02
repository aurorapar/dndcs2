namespace Dndcs2.dice;

public class DieRoll
{
    public int Sides { get; private set; }
    public int Amount { get; private set; }
    public bool Critical { get; private set; }= false;
    public bool CriticalFailure { get; private set; }= false;
    public int Result { get; private set; }
    public int Modifier { get; private set; }
    public bool Advantage { get; private set; }
    public int MinimumResultAllowed { get; private set; }
    public int RerollValue { get; private set; }

    public DieRoll(int sides, int amount, int modifier = 0, int minimumResultAllowed = 1, bool advantage = false, int rerollValue = 0)
    {
        Sides = sides;
        Amount = amount;
        Modifier = modifier;
        MinimumResultAllowed = minimumResultAllowed;
        Advantage = advantage;
        RerollValue = rerollValue;

        Roll();
    }

    private void Roll()
    {
        Result = 0;
        Random rnd = new Random();
        for (int i = 0; i < Amount; i++)
        {
            int result = rnd.Next(0, Sides) + 1;
            if (Sides == 20 && result == 1 && !Advantage)
            {
                CriticalFailure = true;
                Result = result;
                return;
            }

            if (result == 20 && Sides == 20)
            {
                Critical = true;
                Result = result;
                return;
            }
            if (!(Sides == 20 && result == 1))
            {
                while (result < MinimumResultAllowed)
                    result = rnd.Next(0, Sides) + 1;
                if(result < RerollValue)
                    result = rnd.Next(0, Sides) + 1;
            }

            if (Advantage)
            {
                int result2 = rnd.Next(0, Sides) + 1;
                if (!(Sides == 20 && result2 == 1))
                {
                    if (Sides == 20 && result2 == 20)
                    {
                        Critical = true;
                        Result = result2;
                        return;
                    }
                    while(result2 < MinimumResultAllowed)
                        result2 = rnd.Next(0, Sides) + 1;
                    if (result2 < RerollValue)
                        result2 = rnd.Next(0, Sides) + 1;
                }

                result = Math.Max(result, result2) + Modifier;
            }

            Result += result;
        }
    }
}