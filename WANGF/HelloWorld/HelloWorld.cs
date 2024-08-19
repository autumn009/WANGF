using ANGFLib;

namespace HelloWorld
{
    public class HelloWorldModule : ANGFLib.Module
    {
        public override string Id => "{db41d86b-fff7-460e-b682-bb8e7ea3c756}";
        public override string GetStartPlace() => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override Place[] GetPlaces() => new Place[] { new HelloWorldPlace() };
    }
    public class HelloWorldPlace : ANGFLib.Place
    {
        public override string HumanReadableName => "ハロー・ワールドを言う場所";
        public override string Id => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("Say Hello World", () =>
            {
                DefaultPersons.独白.Say("Hello World");
                return true;
            }));
            return base.ConstructMenu(list);
        }
    }
    public class HelloWorldModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new HelloWorldModule() as T };
            return new T[0];
        }
    }
}
