import React, { useRef, useState } from 'react';
import {
  Box,
  Stepper,
  Step,
  StepLabel,
  StepConnector,
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { StepIconProps } from '@mui/material/StepIcon';
import SettingsIcon from '@mui/icons-material/Settings';
import GroupAddIcon from '@mui/icons-material/GroupAdd';
import VideoLabelIcon from '@mui/icons-material/VideoLabel';
import { stepConnectorClasses } from '@mui/material/StepConnector';

import IndicatorForm, { IndicatorFormRef } from './IndicatorForm';
import IndicatorAssignToCategory from './IndicatorAssignToCategory';
import IndicatorAssignToClaim, { IndicatorClaimAssignmentRef } from './IndicatorAssignToClaim';

import { SaveIndicatorChange, SaveIndicatorClaims, SaveIndicatorCategories } from '../api/IndicatorCollection';

const steps = ['اطلاعات شاخص', 'تخصیص دسته‌بندی', 'تخصیص ادعا'];

const ColorlibConnector = styled(StepConnector)(({ theme }) => ({
  [`&.${stepConnectorClasses.alternativeLabel}`]: {
    top: 22,
    left: 'calc(50% + 16px)',
    right: 'calc(-50% + 16px)',
  },
  [`& .${stepConnectorClasses.line}`]: {
    height: 3,
    border: 0,
    borderRadius: 1,
    backgroundColor: theme.palette.mode === 'dark' ? theme.palette.grey[800] : '#eaeaf0',
  },
  [`&.${stepConnectorClasses.active} .${stepConnectorClasses.line}`]: {
    backgroundImage: 'linear-gradient(95deg, rgb(242,113,33), rgb(233,64,87), rgb(138,35,135))',
  },
  [`&.${stepConnectorClasses.completed} .${stepConnectorClasses.line}`]: {
    backgroundImage: 'linear-gradient(95deg, rgb(242,113,33), rgb(233,64,87), rgb(138,35,135))',
  },
}));

const ColorlibStepIconRoot = styled('div')<{
  ownerState: { active?: boolean; completed?: boolean };
}>(({ theme, ownerState }) => ({
  backgroundColor: theme.palette.mode === 'dark' ? theme.palette.grey[700] : '#ccc',
  zIndex: 1,
  color: '#fff',
  width: 50,
  height: 50,
  display: 'flex',
  borderRadius: '50%',
  justifyContent: 'center',
  alignItems: 'center',
  ...(ownerState.active && {
    backgroundImage: 'linear-gradient(136deg, rgb(242,113,33), rgb(233,64,87), rgb(138,35,135))',
    boxShadow: '0 4px 10px 0 rgba(0,0,0,.25)',
  }),
  ...(ownerState.completed && {
    backgroundImage: 'linear-gradient(136deg, rgb(242,113,33), rgb(233,64,87), rgb(138,35,135))',
  }),
}));

function ColorlibStepIcon(props: StepIconProps) {
  const { active, completed, className, icon } = props;
  const icons: { [key: string]: React.ReactElement } = {
    1: <SettingsIcon />,
    2: <GroupAddIcon />,
    3: <VideoLabelIcon />,
  };

  return (
    <ColorlibStepIconRoot ownerState={{ completed, active }} className={className}>
      {icons[String(icon)]}
    </ColorlibStepIconRoot>
  );
}

export default function IndicatorWizardStepper() {
  const [activeStep, setActiveStep] = useState(0);
  const [indicatorId, setIndicatorId] = useState('');

  const indicatorFormRef = useRef<IndicatorFormRef>(null);
  const claimRef = useRef<IndicatorClaimAssignmentRef>(null);

  const handleNext = () => setActiveStep((prev) => prev + 1);
  const handleBack = () => setActiveStep((prev) => prev - 1);
  const handleReset = () => {
    setActiveStep(0);
    setIndicatorId('');
  };

  const handleSaveStep1 = async (request: any) => {
    try {
      const result = await SaveIndicatorChange(request);
      if (result?.id) {
        setIndicatorId(result.id);
        handleNext();
      } else {
        alert('ثبت شاخص ناموفق بود');
      }
    } catch (err) {
      console.error(err);
      alert('خطا در ذخیره‌سازی شاخص');
    }
  };

  const handleSaveClaims = async (request: any) => {
    try {
      await SaveIndicatorClaims(request);
      alert('ذخیره‌سازی ادعاها با موفقیت انجام شد');
      handleReset();
    } catch (err) {
      console.error(err);
      alert('خطا در ذخیره‌سازی ادعاها');
    }
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Stepper alternativeLabel activeStep={activeStep} connector={<ColorlibConnector />}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel StepIconComponent={ColorlibStepIcon}>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Box sx={{ mt: 4 }}>
        {activeStep === 0 && (
          <>
            <IndicatorForm
              ref={indicatorFormRef}
              onSave={(req) => handleSaveStep1(req)}
            />
          </>
        )}

        {activeStep === 1 && (
          <IndicatorAssignToCategory
            indicatorId={indicatorId}
            onNext={handleNext}
            onBack={handleBack}
            onSave={async (req) => {
              // ensure indicatorId is present
              if (!req.indicatorId) req.indicatorId = indicatorId;
              await SaveIndicatorCategories(req);
            }}
          />
        )}

        {activeStep === 2 && (
          <>
            <IndicatorAssignToClaim
              ref={claimRef}
              initialIndicatorId={indicatorId}
              onBack={handleBack}
              onSave={async (req) => {
                // ensure indicator id
                if (!req.indicatorId) req.indicatorId = indicatorId;
                await handleSaveClaims(req as any);
              }}
            />
          </>
        )}
      </Box>
    </Box>
  );
}
