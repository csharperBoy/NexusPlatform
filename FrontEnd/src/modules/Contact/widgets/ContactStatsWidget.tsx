//src/modules/Contact/widgets/ContactStatsWidget.tsx

import React from 'react';
import Card from '@/core/components/Card';

const ContactStatsWidget: React.FC = () => {
  // می‌توانید داده‌ها را از API دریافت کنید (با useQuery)
  return (
    <Card className="p-4">
      <h3 className="font-bold text-lg">آمار کارمندان</h3>
      <p className="text-3xl text-blue-600">20</p>
      <p className="text-sm text-gray-500">کارمند فعال</p>
    </Card>
  );
};

export default ContactStatsWidget;