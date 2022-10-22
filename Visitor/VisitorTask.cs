using System;
using System.Collections.Generic;
using System.Linq;


namespace Inheritance.Geometry.Visitor
{
    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
            MinX = position.X - Radius;
            MaxX = position.X + Radius;
            MinY = position.Y - Radius;
            MaxY = position.Y + Radius;
            MinZ = position.Z - Radius;
            MaxZ = position.Z + Radius;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var vector = point - Position;
            var length2 = vector.GetLength2();
            return length2 <= Radius * Radius;
        }

        public override RectangularCuboid GetBoundingBox()
            => new RectangularCuboid(Position, 2 * Radius, 2 * Radius, 2 * Radius);

        public override Body Accept(IVisitor visitor) => visitor.VisitBall(this);
    }

    public class RectangularCuboid : Body
    {
        public double SizeX { get; }
        public double SizeY { get; }
        public double SizeZ { get; }

        public RectangularCuboid(Vector3 position, double sizeX, double sizeY, double sizeZ) : base(position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            MinX = position.X - sizeX / 2;
            MaxX = position.X + sizeX / 2;
            MinY = position.Y - sizeY / 2;
            MaxY = position.Y + sizeY / 2;
            MinZ = position.Z - sizeZ / 2;
            MaxZ = position.Z + sizeZ / 2;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var minPoint = new Vector3(
                Position.X - SizeX / 2,
                Position.Y - SizeY / 2,
                Position.Z - SizeZ / 2);
            var maxPoint = new Vector3(
                Position.X + SizeX / 2,
                Position.Y + SizeY / 2,
                Position.Z + SizeZ / 2);

            return point >= minPoint && point <= maxPoint;
        }

        public override RectangularCuboid GetBoundingBox() => this;
        public override Body Accept(IVisitor visitor) => visitor.VisitRectangularCuboid(this);
    }

    public class Cylinder : Body
    {
        public double SizeZ { get; }
        public double Radius { get; }

        public Cylinder(Vector3 position, double sizeZ, double radius) : base(position)
        {
            SizeZ = sizeZ;
            Radius = radius;
            MinX = position.X - radius;
            MaxX = position.X + radius;
            MinY = position.Y - radius;
            MaxY = position.Y + radius;
            MinZ = position.Z - sizeZ / 2;
            MaxZ = position.Z + sizeZ / 2;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var vectorX = point.X - Position.X;
            var vectorY = point.Y - Position.Y;
            var length2 = vectorX * vectorX + vectorY * vectorY;
            var minZ = Position.Z - SizeZ / 2;
            var maxZ = minZ + SizeZ;

            return length2 <= Radius * Radius && point.Z >= minZ && point.Z <= maxZ;
        }

        public override RectangularCuboid GetBoundingBox()
            => new RectangularCuboid(Position, 2 * Radius, 2 * Radius, SizeZ);

        public override Body Accept(IVisitor visitor) => visitor.VisitCylinder(this);
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }

        public CompoundBody(IReadOnlyList<Body> parts) : base(parts[0].Position)
        {
            Parts = parts;
            MinX = Parts.Min(p => p.MinX);
            MaxX = Parts.Max(p => p.MaxX);
            MinY = Parts.Min(p => p.MinY);
            MaxY = Parts.Max(p => p.MaxY);
            MinZ = Parts.Min(p => p.MinZ);
            MaxZ = Parts.Max(p => p.MaxZ);
        }

        public override bool ContainsPoint(Vector3 point) => Parts.Any(body => body.ContainsPoint(point));

        public override RectangularCuboid GetBoundingBox()
        {
            var sizeX = Math.Abs(MaxX - MinX);
            var sizeY = Math.Abs(MaxY - MinY);
            var sizeZ = Math.Abs(MaxZ - MinZ);
            var position = new Vector3((MaxX + MinX) / 2, (MaxY + MinY) / 2, (MaxZ + MinZ) / 2);
            return new RectangularCuboid(position, sizeX, sizeY, sizeZ);
        }

        public override Body Accept(IVisitor visitor) => visitor.VisitCompoundBody(this);
    }

    /// <summary>
    /// BoundingBoxVisitor вычисляет минимальный ограничивающий прямоугольный параллелепипед.
    /// </summary>
    public class BoundingBoxVisitor : IVisitor
    {
        public Body VisitBall(Ball ball) => ball.GetBoundingBox();
        public Body VisitRectangularCuboid(RectangularCuboid rectangularCuboid) => rectangularCuboid.GetBoundingBox();
        public Body VisitCylinder(Cylinder cylinder) => cylinder.GetBoundingBox();
        public Body VisitCompoundBody(CompoundBody compoundBody) => compoundBody.GetBoundingBox();
    }

    /// <summary>
    /// BoxifyVisitor заменяет все тела, кроме CompoundBody, на их ограничивающие прямоугольные параллелепипеды.
    /// </summary>
    public class BoxifyVisitor : IVisitor
    {
        public Body VisitBall(Ball ball) => ball.GetBoundingBox();
        public Body VisitRectangularCuboid(RectangularCuboid rectangularCuboid) => rectangularCuboid.GetBoundingBox();
        public Body VisitCylinder(Cylinder cylinder) => cylinder.GetBoundingBox();

        public Body VisitCompoundBody(CompoundBody compoundBody)
            => new CompoundBody(compoundBody.Parts.Select(part => part.Accept(this)).ToList());
    }
}