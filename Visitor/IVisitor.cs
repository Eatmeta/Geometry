namespace Inheritance.Geometry.Visitor
{
    public interface IVisitor
    {
        Body VisitBall(Ball ball);
        Body VisitRectangularCuboid(RectangularCuboid rectangularCuboid);
        Body VisitCylinder(Cylinder cylinder);
        Body VisitCompoundBody(CompoundBody compoundBody);
    }
}