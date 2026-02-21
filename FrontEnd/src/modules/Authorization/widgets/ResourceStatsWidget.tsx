//src/modules/Authorization/widgets/ResourceStatsWidget.tsx

import React from 'react';
import Card from '@/core/components/Card';

const ResourceStatsWidget: React.FC = () => {
  // می‌توانید داده‌ها را از API دریافت کنید (با useQuery)
  return (
    <Card className="p-4">
      <h3 className="font-bold text-lg">آمار منابع</h3>
      <p className="text-3xl text-blue-600">20</p>
      <p className="text-sm text-gray-500">منبع فعال</p>
    </Card>
  );
};

export default ResourceStatsWidget;