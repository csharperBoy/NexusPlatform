// modules/DashboardCore/components/WidgetRenderer.tsx
import React, { Suspense } from 'react';
import { DashboardWidget } from '../types';

interface WidgetRendererProps {
  widget: DashboardWidget;
  fallback?: React.ReactNode;
}

const WidgetRenderer: React.FC<WidgetRendererProps> = ({ widget, fallback = <div>Loading...</div> }) => {
  const Component = widget.component;
  return (
    <Suspense fallback={fallback}>
      <Component {...widget.props} />
    </Suspense>
  );
};

export default WidgetRenderer;