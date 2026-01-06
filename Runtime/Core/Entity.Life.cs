namespace Yogurt
{
    public partial struct Entity
    {
        public Life Life() 
            => WorldFacade.GetLife(this);
    }
}