﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Manipulator.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: Ms-PL
// </copyright>
// <summary>
//   An abstract base class for manipulators.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// An abstract base class for manipulators.
    /// </summary>
    public abstract class Manipulator : UIElement3D
    {
        #region Constants and Fields

        /// <summary>
        ///   The color property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(Manipulator), new UIPropertyMetadata(ColorChanged));

        /// <summary>
        ///   The offset property.
        /// </summary>
        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset", 
            typeof(Vector3D), 
            typeof(Manipulator), 
            new FrameworkPropertyMetadata(
                new Vector3D(0, 0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PositionChanged));

        /// <summary>
        ///   The pivot point property.
        /// </summary>
        public static readonly DependencyProperty PivotProperty = DependencyProperty.Register(
            "Pivot", typeof(Point3D), typeof(Manipulator), new PropertyMetadata(new Point3D()));

        /// <summary>
        ///   The position property.
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position", 
            typeof(Point3D), 
            typeof(Manipulator), 
            new FrameworkPropertyMetadata(
                new Point3D(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PositionChanged));

        /// <summary>
        ///   The target transform property.
        /// </summary>
        public static readonly DependencyProperty TargetTransformProperty =
            DependencyProperty.Register(
                "TargetTransform", 
                typeof(Transform3D), 
                typeof(Manipulator), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///   The value property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", 
            typeof(double), 
            typeof(Manipulator), 
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Manipulator" /> class.
        /// </summary>
        protected Manipulator()
        {
            this.Model = new GeometryModel3D();
            this.Visual3DModel = this.Model;
            this.OnGeometryChanged();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the color of the manipulator.
        /// </summary>
        /// <value> The color. </value>
        public Color Color
        {
            get
            {
                return (Color)this.GetValue(ColorProperty);
            }

            set
            {
                this.SetValue(ColorProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the offset of the visual (this vector is added to the Position point).
        /// </summary>
        /// <value> The offset. </value>
        public Vector3D Offset
        {
            get
            {
                return (Vector3D)this.GetValue(OffsetProperty);
            }

            set
            {
                this.SetValue(OffsetProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the pivot point of the manipulator.
        /// </summary>
        /// <value> The position. </value>
        public Point3D Pivot
        {
            get
            {
                return (Point3D)this.GetValue(PivotProperty);
            }

            set
            {
                this.SetValue(PivotProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the position of the manipulator.
        /// </summary>
        /// <value> The position. </value>
        public Point3D Position
        {
            get
            {
                return (Point3D)this.GetValue(PositionProperty);
            }

            set
            {
                this.SetValue(PositionProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets TargetTransform.
        /// </summary>
        public Transform3D TargetTransform
        {
            get
            {
                return (Transform3D)this.GetValue(TargetTransformProperty);
            }

            set
            {
                this.SetValue(TargetTransformProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the manipulator value.
        /// </summary>
        /// <value> The value. </value>
        public double Value
        {
            get
            {
                return (double)this.GetValue(ValueProperty);
            }

            set
            {
                this.SetValue(ValueProperty, value);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the camera.
        /// </summary>
        protected ProjectionCamera Camera { get; set; }

        /// <summary>
        ///   Gets or sets the hit plane normal.
        /// </summary>
        protected Vector3D HitPlaneNormal { get; set; }

        /// <summary>
        ///   Gets or sets the model.
        /// </summary>
        protected GeometryModel3D Model { get; set; }

        /// <summary>
        ///   Gets or sets the parent viewport.
        /// </summary>
        protected Viewport3D ParentViewport { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Binds this manipulator to a given Visual3D.
        /// </summary>
        /// <param name="source">
        /// Source Visual3D which receives the manipulator transforms. 
        /// </param>
        public virtual void Bind(ModelVisual3D source)
        {
            BindingOperations.SetBinding(this, TargetTransformProperty, new Binding("Transform") { Source = source });
            BindingOperations.SetBinding(this, TransformProperty, new Binding("Transform") { Source = source });
        }

        /// <summary>
        /// Releases the binding of this manipulator.
        /// </summary>
        public virtual void UnBind()
        {
            BindingOperations.ClearBinding(this, TargetTransformProperty);
            BindingOperations.ClearBinding(this, TransformProperty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when Geometry is changed.
        /// </summary>
        /// <param name="d">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data. 
        /// </param>
        protected static void GeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Manipulator)d).OnGeometryChanged();
        }

        /// <summary>
        /// Projects the point on the hit plane.
        /// </summary>
        /// <param name="p">
        /// The p. 
        /// </param>
        /// <param name="hitPlaneOrigin">
        /// The hit Plane Origin. 
        /// </param>
        /// <param name="hitPlaneNormal">
        /// The hit plane normal (world coordinate system). 
        /// </param>
        /// <returns>
        /// The point in world coordinates. 
        /// </returns>
        protected virtual Point3D? GetHitPlanePoint(Point p, Point3D hitPlaneOrigin, Vector3D hitPlaneNormal)
        {
            return Viewport3DHelper.UnProject(this.ParentViewport, p, hitPlaneOrigin, hitPlaneNormal);
        }

        /// <summary>
        /// The on geometry changed.
        /// </summary>
        protected abstract void OnGeometryChanged();

        /// <summary>
        /// The on mouse down.
        /// </summary>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.ParentViewport = Visual3DHelper.GetViewport3D(this);
            this.Camera = this.ParentViewport.Camera as ProjectionCamera;
            var projectionCamera = this.Camera;
            if (projectionCamera != null)
            {
                this.HitPlaneNormal = projectionCamera.LookDirection;
            }

            this.CaptureMouse();
        }

        /// <summary>
        /// The on mouse up.
        /// </summary>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.ReleaseMouseCapture();
        }

        /// <summary>
        /// Transforms from world to local coordinates.
        /// </summary>
        /// <param name="worldPoint">
        /// The point (world coords). 
        /// </param>
        /// <returns>
        /// Transformed vector (local coords). 
        /// </returns>
        protected Point3D ToLocal(Point3D worldPoint)
        {
            var mat = Visual3DHelper.GetTransform(this);
            mat.Invert();
            var t = new MatrixTransform3D(mat);
            return t.Transform(worldPoint);
        }

        /// <summary>
        /// Transforms from local to world coordinates.
        /// </summary>
        /// <param name="point">
        /// The point (local coords). 
        /// </param>
        /// <returns>
        /// Transformed point (world coords). 
        /// </returns>
        protected Point3D ToWorld(Point3D point)
        {
            var mat = Visual3DHelper.GetTransform(this);
            var t = new MatrixTransform3D(mat);
            return t.Transform(point);
        }

        /// <summary>
        /// Transforms from local to world coordinates.
        /// </summary>
        /// <param name="vector">
        /// The vector (local coords). 
        /// </param>
        /// <returns>
        /// Transformed vector (world coords). 
        /// </returns>
        protected Vector3D ToWorld(Vector3D vector)
        {
            var mat = Visual3DHelper.GetTransform(this);
            var t = new MatrixTransform3D(mat);
            return t.Transform(vector);
        }

        /// <summary>
        /// The color changed.
        /// </summary>
        /// <param name="d">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Manipulator)d).OnColorChanged();
        }

        /// <summary>
        /// Called when position has been changed.
        /// </summary>
        /// <param name="d">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data. 
        /// </param>
        private static void PositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Manipulator)d).OnPositionChanged();
        }

        /// <summary>
        /// The on color changed.
        /// </summary>
        private void OnColorChanged()
        {
            this.Model.Material = MaterialHelper.CreateMaterial(this.Color);
            this.Model.BackMaterial = this.Model.Material;
        }

        /// <summary>
        /// Called when position is changed.
        /// </summary>
        private void OnPositionChanged()
        {
            this.Transform = new TranslateTransform3D(this.Position.X + this.Offset.X, this.Position.Y + this.Offset.Y, this.Position.Z + this.Offset.Z);
        }

        #endregion
    }
}